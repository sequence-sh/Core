namespace Reductech.Sequence.Core.LanguageServer.Objects;

public class LinePositionSpanTextChange
{
    public string NewText { get; set; } = "";

    public int StartLine { get; set; }

    public int StartColumn { get; set; }

    public int EndLine { get; set; }

    public int EndColumn { get; set; }

    public override bool Equals(object? obj) =>
        obj is LinePositionSpanTextChange positionSpanTextChange
     && NewText == positionSpanTextChange.NewText && StartLine == positionSpanTextChange.StartLine
     && StartColumn == positionSpanTextChange.StartColumn
     && EndLine == positionSpanTextChange.EndLine && EndColumn == positionSpanTextChange.EndColumn;

    public override int GetHashCode() => NewText.GetHashCode() * (23 + StartLine)
                                                               * (29 + StartColumn) * (31 + EndLine)
                                                               * (37 + EndColumn);

    public override string ToString() =>
        $"StartLine={StartLine}, StartColumn={StartColumn}, EndLine={EndLine}, EndColumn={EndColumn}, NewText='{(NewText.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"))}'";
}
