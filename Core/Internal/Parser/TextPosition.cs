using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal.Parser
{

/// <summary>
/// A position within a piece of text.
/// </summary>
public sealed record TextPosition(int Line, int Column, int Index)
{
    /// <summary>
    /// Create a TextPosition from a token
    /// </summary>
    public TextPosition(IToken token) : this(token.Line, token.Column, token.StartIndex) { }

    /// <inheritdoc />
    public override string ToString() => $"Line: {Line}, Col: {Column}, Idx: {Index}";

    /// <summary>
    /// Create a TextPosition Stop
    /// </summary>
    public static TextPosition CreateStop(IToken token) => new(
        token.Line,
        token.Column + (token.StopIndex - token.StartIndex),
        token.StopIndex
    );
}

/// <summary>
/// A location within a piece of text
/// </summary>
public sealed record TextLocation
    (string Text, TextPosition Start, TextPosition Stop) : IErrorLocation
{
    /// <summary>
    /// Create a textLocation from a token
    /// </summary>
    public TextLocation(IToken token) : this(
        token.Text,
        new TextPosition(token),
        TextPosition.CreateStop(token)
    ) { }

    /// <summary>
    /// Create a textLocation from a context
    /// </summary>
    public TextLocation(ParserRuleContext context) :
        this(
            GetSourceText(context),
            new TextPosition(context.Start),
            TextPosition.CreateStop(context.Stop)
        ) { }

    /// <inheritdoc />
    public bool Equals(IErrorLocation? other) => other is TextLocation tl && Equals(tl);

    /// <inheritdoc />
    public string AsString => $"{Start} - {Stop} Text: {Text}";

    /// <inheritdoc />
    public override string ToString() => AsString;

    static string GetSourceText(ParserRuleContext context)
    {
        var text = context.Start.TokenSource.InputStream.GetText(
            new Interval(context.Start.StartIndex, context.Stop.StopIndex)
        );

        return text;
    }
}

}
