namespace Reductech.Sequence.Core.LanguageServer;

/// <summary>
/// General helper methods for the Language Server
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Root url of the Reductech Documentation Website
    /// </summary>
    public const string DocumentationRootUrl = "https://sequence.sh/steps/";

    /// <summary>
    /// Does this token contain this position
    /// </summary>
    public static bool ContainsPosition(this IToken token, LinePosition position)
    {
        return token.StartsBeforeOrAt(position) && token.EndsAfterOrAt(position);
    }

    /// <summary>
    /// Gets the range of this token
    /// </summary>
    public static TextRange GetRange(this IToken token)
    {
        return new(
            token.Line - 1,
            token.Column,
            token.Line - 1,
            token.Column + token.Text.Length
        );
    }

    /// <summary>
    /// Returns whether this node siblings after the position
    /// </summary>
    public static bool HasSiblingsAfter(this IRuleNode ruleContext, LinePosition p)
    {
        if (ruleContext.Parent is ParserRuleContext prc)
        {
            if (prc.children.Reverse()
                .OfType<ParserRuleContext>()
                .Any(c => c.ContainsPosition(p) || c.StartsAfter(p)))
                return true;

            return HasSiblingsAfter(prc, p);
        }

        return false;
    }

    /// <summary>
    /// Whether this token starts before or at the position
    /// </summary>
    public static bool StartsBeforeOrAt(this IToken token, LinePosition position)
    {
        if (token.Line - 1 < position.Line)
            return true;
        else if (token.Line - 1 == position.Line)
            return token.Column <= position.Character;

        return false;
    }

    /// <summary>
    /// Whether this token ends after or at the position
    /// </summary>
    public static bool EndsAfterOrAt(this IToken token, LinePosition position)
    {
        if (token.Line - 1 < position.Line)
            return false;
        else if (token.Line - 1 == position.Line)
            return (token.Column + token.Text.Length) >= position.Character;

        return true;
    }

    /// <summary>
    /// Whether this token ends at the position
    /// </summary>
    public static bool EndsAt(this IToken token, LinePosition position)
    {
        if (token.Line - 1 < position.Line)
            return false;
        else if (token.Line - 1 == position.Line)
            return (token.Column + token.Text.Length) == position.Character;

        return true;
    }

    /// <summary>
    /// Splits SCL text into commands
    /// </summary>
    public static IReadOnlyList<(string text, LinePosition position)> SplitIntoCommands(string text)
    {
        var inputStream = new AntlrInputStream(text);
        var lexer       = new SCLLexer(inputStream, TextWriter.Null, TextWriter.Null);

        var tokens = lexer.GetAllTokens();

        var newCommandTokenType = lexer.GetTokenType("NEWCOMMAND");

        List<(string text, LinePosition startPosition)> results = new();

        StringBuilder sb    = new();
        LinePosition? start = null;

        foreach (var token in tokens)
        {
            if (token.Type == newCommandTokenType)
            {
                if (start.HasValue)
                {
                    results.Add((sb.ToString(), start.Value));
                }

                sb    = new StringBuilder();
                start = null;
            }

            if (start == null)
            {
                var trimmedText = token.Text;
                var lineOffset  = 0;

                while (true)
                {
                    if (trimmedText.StartsWith('\n'))
                        trimmedText = trimmedText[1..];
                    else if (trimmedText.StartsWith("\r\n"))
                        trimmedText = trimmedText[2..];
                    else
                        break;

                    lineOffset++;
                }

                start = new(token.Line + lineOffset - 1, lineOffset > 0 ? 0 : token.Column);
                sb.Append(trimmedText);
            }
            else
            {
                sb.Append(token.Text);
            }
        }

        if (start.HasValue)
            results.Add((sb.ToString(), start.Value));

        return results;
    }

    /// <summary>
    /// Gets a particular command from the text
    /// </summary>
    public static (string command, LinePosition newPosition, LinePosition positionOffset)?
        GetCommand(
            string text,
            LinePosition originalPosition)
    {
        var commands  = SplitIntoCommands(text);
        var myCommand = commands.TakeWhile(x => x.position <= originalPosition).LastOrDefault();

        if (myCommand == default)
            return null;

        LinePosition newPosition;
        LinePosition offsetPosition;

        if (originalPosition.Line == myCommand.position.Line)
        {
            newPosition = new LinePosition(
                0,
                originalPosition.Character - myCommand.position.Character
            );

            offsetPosition = myCommand.position;
        }
        else
        {
            newPosition = new LinePosition(
                originalPosition.Line - myCommand.position.Line,
                originalPosition.Character
            );

            offsetPosition = myCommand.position;
        }

        return (myCommand.text, newPosition, offsetPosition);
    }

    /// <summary>
    /// Remove a token from the text
    /// </summary>
    public static (string newText, IToken? removed) RemoveToken(
        string text,
        LinePosition tokenPosition)
    {
        var inputStream = new AntlrInputStream(text);
        var lexer       = new SCLLexer(inputStream, TextWriter.Null, TextWriter.Null);

        StringBuilder sb = new();

        IToken? removed = null;

        foreach (var token in lexer.GetAllTokens())
        {
            if (token.ContainsPosition(tokenPosition))
            {
                removed = token;
                var length = token.StopIndex - token.StartIndex;

                var ws = new string(' ', length);
                sb.Append(ws);
            }
            else
            {
                sb.Append(token.Text);
            }
        }

        return (sb.ToString(), removed);
    }

    /// <summary>
    /// Whether the context contains the position
    /// </summary>
    public static bool ContainsPosition(this ParserRuleContext context, LinePosition position)
    {
        if (!context.Start.StartsBeforeOrAt(position))
            return false;

        if (!context.Stop.EndsAfterOrAt(position))
            return false;

        return true;
    }

    /// <summary>
    /// Whether the token is on the same line as the position
    /// </summary>
    public static bool IsSameLineAs(this IToken token, LinePosition position)
    {
        var sameLine = token.Line - 1 == position.Line;
        return sameLine;
    }

    /// <summary>
    /// Whether the context ends before the position
    /// </summary>
    public static bool EndsBefore(this ParserRuleContext context, LinePosition position) =>
        !context.Stop.EndsAfterOrAt(position);

    /// <summary>
    /// Whether the context starts after the position
    /// </summary>
    public static bool StartsAfter(this ParserRuleContext context, LinePosition position) =>
        !context.Start.StartsBeforeOrAt(position);

    /// <summary>
    /// Get the range of the context
    /// </summary>
    public static TextRange GetRange(this ParserRuleContext context)
    {
        return new(
            context.Start.Line - 1,
            context.Start.Column,
            context.Stop.Line - 1,
            context.Stop.Column + context.Stop.Text.Length
        );
    }

    /// <summary>
    /// Get the range of the Text Location
    /// </summary>
    public static TextRange GetRange(this TextLocation textLocation, int lineOffset, int charOffSet)
    {
        return new(
            textLocation.Start.ToLinePosition().GetFromOffset(lineOffset, charOffSet),
            textLocation.Stop.ToLinePosition().GetFromOffset(lineOffset, charOffSet)
        );
    }

    /// <summary>
    /// Offsets the range by the position
    /// </summary>
    public static TextRange Offset(this TextRange range, LinePosition offset)
    {
        return new TextRange(
            new LinePosition(offset.Line + range.StartLineNumber, range.StartColumn),
            new LinePosition(offset.Line + range.EndLineNumber,   range.EndColumn)
        );
    }

    public static LinePosition ToLinePosition(this TextPosition textPosition)
    {
        return new LinePosition(textPosition.Line, textPosition.Column);
    }

    /// <summary>
    /// Get the position adjusted by the offset
    /// </summary>
    public static LinePosition GetFromOffset(
        this LinePosition position,
        int lineOffset,
        int charOffSet)
    {
        if (position.Line == 1)
            //same line, add columns
            return new LinePosition(lineOffset, position.Character + charOffSet);
        else //add lines
            return new LinePosition(position.Line - 1 + lineOffset, position.Character);
    }

    /// <summary>
    /// Lex parse and Visit the SCL text
    /// </summary>
    public static T LexParseAndVisit<T>(
        this SCLBaseVisitor<T> visitor,
        string text,
        Action<SCLLexer> setupLexer,
        Action<SCLParser> setupParser)
    {
        var inputStream       = new AntlrInputStream(text);
        var lexer             = new SCLLexer(inputStream, TextWriter.Null, TextWriter.Null);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser            = new SCLParser(commonTokenStream, TextWriter.Null, TextWriter.Null);

        setupLexer(lexer);
        setupParser(parser);

        var result = visitor.Visit(parser.fullSequence());

        return result;
    }

    /// <summary>
    /// Get the documentation of the step
    /// </summary>
    public static string GetMarkDownDocumentation(IStepFactory stepFactory, string rootUrl)
    {
        var grouping = new[] { stepFactory }
            .GroupBy(x => x, x => x.TypeName)
            .Single();

        return GetMarkDownDocumentation(grouping, rootUrl);
    }

    /// <summary>
    /// Get the documentation of the step
    /// </summary>
    public static string GetMarkDownDocumentation(
        IGrouping<IStepFactory, string> stepFactoryGroup,
        string rootUrl)
    {
        try
        {
            var stepWrapper = new StepWrapper(stepFactoryGroup);
            var text        = DocumentationCreator.GetStepPage(stepWrapper, rootUrl);

            return text.FileText;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}
