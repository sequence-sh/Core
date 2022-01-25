namespace Reductech.Sequence.Core.LanguageServer.Objects;

public readonly record struct LinePosition(int Line, int Character)
{
    public static LinePosition Zero => new();

    public static bool operator <(LinePosition a, LinePosition b)
    {
        if (a.Line < b.Line)
            return true;

        if (a.Line == b.Line && a.Character < b.Character)
            return true;

        return false;
    }

    public static bool operator >(LinePosition a, LinePosition b)
    {
        if (a.Line > b.Line)
            return true;

        if (a.Line == b.Line && a.Character > b.Character)
            return true;

        return false;
    }

    public static bool operator <=(LinePosition a, LinePosition b)
    {
        if (a.Line < b.Line)
            return true;

        if (a.Line == b.Line && a.Character <= b.Character)
            return true;

        return false;
    }

    public static bool operator >=(LinePosition a, LinePosition b)
    {
        if (a.Line > b.Line)
            return true;

        if (a.Line == b.Line && a.Character >= b.Character)
            return true;

        return false;
    }
}
