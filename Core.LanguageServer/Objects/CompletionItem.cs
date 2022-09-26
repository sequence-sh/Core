namespace Reductech.Sequence.Core.LanguageServer.Objects;

/// <summary>
/// A code completion item
/// </summary>
public record CompletionItem(
    string Label,
    string Detail,
    string Documentation,
    bool Preselect,
    SCLTextEdit TextEdit)
{
    /// <summary>
    /// Offset this completion item by a line offset
    /// </summary>
    public CompletionItem Offset(LinePosition linePosition) =>
        this with { TextEdit = TextEdit.Offset(linePosition) };
}
