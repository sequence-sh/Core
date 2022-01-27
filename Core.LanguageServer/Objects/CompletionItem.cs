namespace Reductech.Sequence.Core.LanguageServer.Objects;

public record CompletionItem(
    string Label,
    string Detail,
    string Documentation,
    bool Preselect,
    SCLTextEdit TextEdit)
{
    public CompletionItem Offset(LinePosition linePosition) =>
        this with { TextEdit = TextEdit.Offset(linePosition) };
}
