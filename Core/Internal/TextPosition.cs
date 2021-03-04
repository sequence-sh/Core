using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A position within a piece of text.
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed record TextPosition(int Line, int Column, int Index) : IComparable<TextPosition>
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <summary>
    /// Create a TextPosition from a token
    /// </summary>
    public TextPosition(IToken token) : this(token.Line, token.Column, token.StartIndex) { }

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

        if (ReferenceEquals(null, other))
            return 1;

        return Index.CompareTo(other.Index);
    }
}

}
