namespace Reductech.Sequence.Core.LanguageServer.Objects;

/// <summary>
/// A position in SCL text.
/// The first line is line 1.
/// </summary>
public readonly record struct LinePosition(int Line, int Character)
    : IComparable<LinePosition>
{
    /// <summary>
    /// Less than operator
    /// </summary>
    public static bool operator <(LinePosition a, LinePosition b)
    {
        if (a.Line < b.Line)
            return true;

        if (a.Line == b.Line && a.Character < b.Character)
            return true;

        return false;
    }

    /// <summary>
    /// Greater than operator
    /// </summary>
    public static bool operator >(LinePosition a, LinePosition b)
    {
        if (a.Line > b.Line)
            return true;

        if (a.Line == b.Line && a.Character > b.Character)
            return true;

        return false;
    }

    /// <summary>
    /// Less than or equal operator
    /// </summary>
    public static bool operator <=(LinePosition a, LinePosition b)
    {
        if (a.Line < b.Line)
            return true;

        if (a.Line == b.Line && a.Character <= b.Character)
            return true;

        return false;
    }

    /// <summary>
    /// Greater than or equal operator
    /// </summary>
    public static bool operator >=(LinePosition a, LinePosition b)
    {
        if (a.Line > b.Line)
            return true;

        if (a.Line == b.Line && a.Character >= b.Character)
            return true;

        return false;
    }

    /// <inheritdoc />
    int IComparable<LinePosition>.CompareTo(LinePosition other)
    {
        if (this == other)
            return 0;

        if (this < other)
            return -1;

        return 1;
    }
}
