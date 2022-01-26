namespace Reductech.Sequence.Core.LanguageServer.Objects;

public class QuickInfoResponse
{
    public string Markdown => string.Join("\r\n", MarkdownStrings);

    public List<string> MarkdownStrings { get; set; } = new();
}
