namespace Reductech.Sequence.Core.LanguageServer.Objects;

/// <summary>
/// The response from a code completion request
/// </summary>
public record CompletionResponse(bool IsIncomplete, IReadOnlyList<CompletionItem> Items)
{
    /// <summary>
    /// Offset this completion response by a line offset
    /// </summary>
    public CompletionResponse Offset(LinePosition linePosition) => this with
    {
        Items = Items.Select(x => x.Offset(linePosition)).ToList()
    };
}
