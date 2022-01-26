namespace Reductech.Sequence.Core.LanguageServer.Objects;

public class CompletionItem
{
    public string Label { get; set; }

    public CompletionItemKind Kind { get; set; }

    public IReadOnlyList<CompletionItemTag>? Tags { get; set; }

    public string? Detail { get; set; }

    public string? Documentation { get; set; }

    public bool Preselect { get; set; }

    public string? SortText { get; set; }

    public string? FilterText { get; set; }

    public InsertTextFormat? InsertTextFormat { get; set; }

    public LinePositionSpanTextChange? TextEdit { get; set; }

    public IReadOnlyList<char>? CommitCharacters { get; set; }

    public IReadOnlyList<LinePositionSpanTextChange>? AdditionalTextEdits { get; set; }

    public int Data { get; set; }

    public override string ToString() => $"{{ Label = {Label}, CompletionItemKind = {Kind} }}";
}
