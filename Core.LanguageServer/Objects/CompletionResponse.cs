namespace Reductech.Sequence.Core.LanguageServer.Objects;

public record CompletionResponse(bool IsIncomplete, IReadOnlyList<CompletionItem> Items)
{
    public CompletionResponse Offset(LinePosition linePosition) => this with
    {
        Items = Items.Select(x => x.Offset(linePosition)).ToList()
    };
}
