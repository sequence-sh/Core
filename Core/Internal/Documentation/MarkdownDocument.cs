namespace Reductech.EDR.Core.Internal.Documentation
{

/// <summary>
/// A Document Written in MarkDown
/// </summary>
public record MarkdownDocument(
    string FileName,
    string Title,
    string FileText,
    string Directory,
    string Category);

}
