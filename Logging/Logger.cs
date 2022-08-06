using System.IO;

public static class Logger
{
    public static Stack<LotusError> errorStack = new();

    public static int ErrorCount => errorStack.Count;

    public static bool HasErrors => errorStack.Count != 0;

    public static Dictionary<string, ISourceCodeProvider> providers = new();

    public static void RegisterSourceProvider(ISourceCodeProvider prov) {
        var success = providers.TryAdd(prov.Filename, prov);

        Debug.Assert(success);
    }

    public static void Log(string message, LocationRange location)
        => Console.WriteLine($"{location}: {message}");

    public static void Warning(string message, LocationRange location)
        => Console.Error.WriteLine(location + " : " + message);

    public static void Warning(LotusError e)
        => Console.Error.WriteLine("WARNING : " + Format(e));

    public static void Error(LotusError e)
        => errorStack.Push(e);

    public static Exception Fatal(Exception e) {
        if (ErrorCount > 0)
            PrintAllErrors();

        return e; // TODO: Do fancy stuff with method (like pretty-printing the exception)
    }

    public static void PrintAllErrors() {
        var sb = new MarkupBuilder();

        var orderedErrorStack = errorStack.Reverse()/*.OrderBy(e => {
            if (e is ILocalized el) return el.Location;
            else return LocationRange.NULL;
        })*/;

        // TODO: we could optimize this function to call Format() in parallel and then Thread.WaitAll() to then print them
        // Problems :
        //      - They won't be in order anymore (because we'll have to use a ConcurrentBag)
        foreach (var error in orderedErrorStack) {
            sb.AppendLine();

            var errorTypeString =
                  error.GetType().GetDisplayName()
#if DEBUG
                + " @ "
                + GetCallerString(error)
#endif
                ;

            // TODO: Why not try to center the names when there's multiple exceptions ?
            var frontCharCount = Console.WindowWidth - errorTypeString.Length;

            if (frontCharCount > 2) {
                sb.PushTextFormat(TextFormat.Faint);
                var backCharCount = Console.WindowWidth / 5;

                var backChars = "";

                if (frontCharCount > backCharCount) {
                    backChars = " " + new string('-', backCharCount);

                    sb.Append(new string('-', frontCharCount - 2 - backCharCount));
                }

                sb.Append(" " + errorTypeString,
                    new Style(
                        Foreground: TextColor.RedColor,
                        Format: TextFormat.Reset
                    )
                );

                sb.AppendLine(backChars + "\n");

                sb.PopTextFormat();
            }

            sb.Append(Format(error));
        }

        sb.PushTextFormat(TextFormat.Bold | TextFormat.Italic | TextFormat.Underline);

        if (ErrorCount == 1) {
            sb.AppendLine("There was an error, please fix it before proceeding.");
        } else if (ErrorCount > 1) {
            sb.Append("There were ");
            sb.Append(ErrorCount, TextColor.BlueColor);
            sb.AppendLine(" build errors. Fix them before proceeding.");
        }

        sb.PopTextFormat();

        Console.Error.Write(TerminalCompliantStringOf(sb));
    }

    private static string TerminalCompliantStringOf(MarkupBuilder sb) {
        if (Environment.GetEnvironmentVariable("NO_COLOR") is not null and not ""
        ||  Console.IsOutputRedirected)
            return sb.ToString();

        return sb.Render();
    }

    public static string FormatError(LotusError error)
        => TerminalCompliantStringOf(Format(error));

    internal static MarkupBuilder Format(LotusError error) {
        var sb = new MarkupBuilder();

        // Interfaces to implement :
        //      - ILocalized
        //      - IContextualized
        //      - UnexpectedError (abstract class)

        // TODO: NotAName handling (e.g. traversing parse tree to print which part makes it "not a name")
        /*if (error is NotANameError eNotAName) {
            sb.AppendLine(FormatNotAName(eNotAName)).AppendLine();
        } else*/ if (error is UnexpectedError eUnx) {
            sb.AppendLine(FormatUnexpected(eUnx)).AppendLine();
        } else if (error is IContextualized eCtx) {
            //* FormatUnexpected already takes care of context for us, so we only do it
            //* if the error is not UnexpectedError
            sb.AppendLine(FormatContextualized(eCtx)).AppendLine();
        }

        if (error is ILocalized eLoc) {
            // TODO: Allow passing a short message to be displayed in the sample
            sb.AppendLine(FormatLocalized(eLoc));
        }

        if (error.Message != null) {
            sb.AppendLine("\n" + error.Message);
        }

        if (error.ExtraNotes != null) {
            sb.AppendLine("\nNote: " + error.ExtraNotes);
        }

        sb.AppendLine();

        return sb;
    }

    internal static MarkupBuilder FormatUnexpected(UnexpectedError error) {
        var sb = new MarkupBuilder("Unexpected ");

        switch (error) {
            case IValued<Token> unxToken:
                var token = unxToken.Value;

                sb.Append(token.Kind);

                if (token.Kind is not TokenKind.EOF) {
                    sb.Append(" '" + ASTHelper.PrintToken(token).Trim() + "'");
                }

                break;
            case IValued<Node> unxNode:
                var node = unxNode.Value;

                if (node is ValueNode.Dummy or StatementNode.Dummy or TopLevelNode.Dummy) {
                    return FormatUnexpected(
                        new UnexpectedError<Token>(
                            error.Area,
                            caller: error.Caller,
                            callerPath: error.CallerPath
                        ) {
                            Value = unxNode.Value.Token,
                            As = error.As,
                            In = error.In,
                            Location = error.Location,
                            Expected = error.Expected,
                            Message = error.Message,
                        }
                    );
                }

                if (node is StatementExpressionNode stmtExprNode) {
                    return FormatUnexpected(
                        new UnexpectedError<ValueNode>(
                            error.Area,
                            caller: error.Caller,
                            callerPath: error.CallerPath
                        ) {
                            Value = stmtExprNode.Value,
                            As = error.As,
                            In = error.In,
                            Location = error.Location,
                            Expected = error.Expected,
                            Message = error.Message,
                        }
                    );
                }

                sb.Append(
                      (node is OperationNode opNode ? opNode.OperationType + " (" + opNode.Token + ')': node.GetType().Name)
                    + " '"
                    + (node.Location.LineLength < 100 ? ASTHelper.PrintNode(node).Trim() : "")
                    + "'"
                );
                break;
            case IValued<string> unxString:
                sb.Append("input '" + unxString.Value.Trim() + "'");
                break;
            case IValued<char> unxChar:
                var chr = unxChar.Value;
                if (chr is '\n' or '\r') {
                    sb.Append("newline character.");
                } else if (chr == '\t') {
                    sb.Append("tabulation character.");
                } else if (Char.IsWhiteSpace(chr)) {
                    sb.Append("whitespace character '" + System.Text.RegularExpressions.Regex.Escape(chr.ToString()));
                } else {
                    sb.Append("character '" + chr + "'.");
                }
                break;
            default:
                sb.Append(String.Join(", ", error.GetType().GenericTypeArguments.Select(t => t.Name)));
                break;
        }

        if (error.In is not null)
            sb.Append(" in " + error.In);

        if (error.As is not null)
            sb.Append(" as " + error.As);

        error.Expected.Match(
            s => sb.Append("\nExpected " + s + "."),
            list => sb.Append("\nExpected one of :\n\t- " + String.Join("\n\t- ", list)),
            _ => sb
        );

        return sb;
    }

    internal static MarkupBuilder FormatContextualized(IContextualized error) {
        return new MarkupBuilder("Error happened in " + error.In);
    }

    internal static MarkupBuilder FormatLocalized(ILocalized error) {
        var sb = new MarkupBuilder();
        var location = error.Location;
        var fileInfo = new FileInfo(location.filename);

        var relPath = "";

        if (fileInfo.Exists) {
            relPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), fileInfo.FullName);
        }

        // TODO: Ideas for formatting the filename :
        //      - Underline the path and put a '@' prefix
        //      - Use the '-->' prefix
        //      - Put in bold

        sb.PushTextFormat(TextFormat.Bold);
        sb.Append($"\t --> @{relPath}({location.firstLine}:{location.firstColumn}");
        if (!location.IsSingleLocation()) {
            if (location.firstLine == location.lastLine)
                sb.Append($" ~ {location.lastColumn}");
            else
                sb.Append($" ~ {location.lastLine}:{location.lastColumn}");
        }
        sb.AppendLine(")");
        sb.PopTextFormat();

        sb.PushForeground(TextColor.BlueColor);

        sb.Append(FormatTextAt(location, error));

        sb.PopForeground();

        return sb;
    }

    private static string FormatTextAt(LocationRange location, in ILocalized error) {
        if (providers.TryGetValue(location.filename, out var sourceCodeProvider)) {
            return FormatTextAt(location, sourceCodeProvider.Source);
        }

        if (File.Exists(location.filename)) {
            var src = new SourceCode(new Uri(location.filename));

            RegisterSourceProvider(new SourceCodeWrapper(location.filename, src));

            return FormatTextAt(location, src);
        }

        string sourceCode;

        if (error is IValued<Node> eNode) {
            sourceCode = ASTHelper.PrintNode(eNode.Value);
        } else if (error is IValued<Token> eToken) {
            sourceCode = ASTHelper.PrintToken(eToken.Value);
        } else if (error is IValued<string> eString) {
            sourceCode = eString.Value;
        } else {
            sourceCode = "";
        }

        return FormatTextAt(
                new LocationRange(
                    firstLine: 1,
                    lastLine: location.lastLine - location.firstLine + 1,
                    firstColumn: 1,
                    lastColumn: location.lastColumn - location.firstColumn + 1,
                    filename: location.filename
                ),
                new SourceCode(sourceCode)
            ) + "\nWARNING : This source code is approximated, because no source code could be found for " + location.filename;
    }

    private static string FormatTextAt(LocationRange range, SourceCode source) {
        if (range.LineLength == 1) {
            if (range.ColumnLength == 1) return source.FormatTextAtPoint(range.GetFirstLocation());
            else return source.FormatTextAtLine(range);
        }

        return source.FormatTextAtLines(range);
    }

    private static string GetCallerString(LotusError error) => Path.GetFileNameWithoutExtension(error.CallerPath) + '.' + error.Caller;

    internal class SourceCodeWrapper : ISourceCodeProvider {
        private string _filename;
        public string Filename => _filename;

        private SourceCode _src;
        public SourceCode Source => _src;

        public SourceCodeWrapper(string filename, SourceCode src) {
            _filename = filename;
            _src = src;
        }
    }
}