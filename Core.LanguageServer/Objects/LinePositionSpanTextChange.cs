namespace Reductech.Sequence.Core.LanguageServer.Objects;

public record LinePositionSpanTextChange(
    string NewText,
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn)
{
    public override string ToString() =>
        $"StartLine={StartLine}, StartColumn={StartColumn}, EndLine={EndLine}, EndColumn={EndColumn}, NewText='{(NewText.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"))}'";

    public LinePositionSpanTextChange(string newText, TextRange range)
        : this(
            newText,
            range.StartLineNumber,
            range.StartColumn,
            range.EndLineNumber,
            range.EndColumn
        ) { }

    /// <summary>
    /// Offset this by some number of lines
    /// </summary>
    public LinePositionSpanTextChange Offset(LinePosition linePosition) => this with
    {
        StartLine = StartLine + linePosition.Line,
        EndLine = EndLine + linePosition.Line,
        StartColumn = StartColumn + linePosition.Character,
        EndColumn = EndColumn + linePosition.Character,
    };
}
