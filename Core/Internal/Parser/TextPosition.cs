using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Reductech.EDR.Core.Internal.Parser //TODO move
{

/// <summary>
/// A position within a piece of text.
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed record TextPosition(int Line, int Column, int Index)
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed record TextLocation(string Text, TextPosition Start, TextPosition Stop)
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

    /// <summary>
    /// The text location as a string
    /// </summary>
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
