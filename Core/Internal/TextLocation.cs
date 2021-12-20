using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A location within a piece of text
/// </summary>
public sealed record TextLocation(string Text, TextPosition Start, TextPosition Stop)
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

    /// <summary>
    /// Whether the location contains this position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool Contains(TextPosition position)
    {
        if (position.CompareTo(Start) >= 0)
            if (position.CompareTo(Stop) <= 0)
                return true;

        return false;
    }
}
