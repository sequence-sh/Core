namespace Reductech.Sequence.Core.LanguageServer.Objects;

/// <summary>
/// The response from a quick info request
/// </summary>
public class QuickInfoResponse
{
    /// <summary>
    /// Markdown strings in the response
    /// </summary>
    public List<string> MarkdownStrings { get; set; } = new();
}
