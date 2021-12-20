using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A position within a piece of text.
/// </summary>
public sealed record TextPosition(int Line, int Column, int Index) : IComparable<TextPosition>
{
    /// <summary>
    /// Create a TextPosition from a token
    /// </summary>
    public TextPosition(IToken token) : this(token.Line, token.Column, token.StartIndex) { }

    /// <summary>
    /// This TextPosition as an Interval
    /// </summary>
    public Interval Interval => new(Index, Index);

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

    /// <inheritdoc />
    public int CompareTo(TextPosition? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (other is null)
            return 1;

        return Index.CompareTo(other.Index);
    }
}
