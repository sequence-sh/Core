namespace Sequence.Core.LanguageServer.Objects;

/// <summary>
/// A language server text edit
/// </summary>
public record SCLTextEdit(
    string NewText,
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn)
{
    /// <summary>
    /// Convert this Text Edit to a string
    /// </summary>
    /// <returns></returns>
    public override string ToString() =>
        $"StartLine={StartLine}, StartColumn={StartColumn}, EndLine={EndLine}, EndColumn={EndColumn}, NewText='{(NewText.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"))}'";

    /// <summary>
    /// Create a new SCLTextEdit
    /// </summary>
    public SCLTextEdit(string newText, TextRange range)
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
    public SCLTextEdit Offset(LinePosition linePosition) => this with
    {
        StartLine = StartLine + linePosition.Line,
        EndLine = EndLine + linePosition.Line,
        StartColumn = StartColumn + linePosition.Character,
        EndColumn = EndColumn + linePosition.Character,
    };
}
