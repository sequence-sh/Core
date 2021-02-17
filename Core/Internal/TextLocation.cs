using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Reductech.EDR.Core.Internal
{

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
