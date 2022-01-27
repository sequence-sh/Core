namespace Reductech.Sequence.Core.LanguageServer.Objects;

public record CompletionItem(
    string Label,
    string Detail,
    string Documentation,
    bool Preselect,
    LinePositionSpanTextChange TextEdit) { }
