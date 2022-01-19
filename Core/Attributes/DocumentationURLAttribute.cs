namespace Reductech.Sequence.Core.Attributes;

/// <summary>
/// Points to a documentation page for this parameter.
/// </summary>
public sealed class DocumentationURLAttribute : StepPropertyMetadataAttribute
{
    /// <summary>
    /// Creates a new DocumentationURLAttribute.
    /// </summary>
    public DocumentationURLAttribute(string documentationURL, string label)
    {
        DocumentationURL = documentationURL;
        Label            = label;
    }

    /// <summary>
    /// The url to the documentation.
    /// </summary>
    public string DocumentationURL { get; }

    /// <summary>
    /// The label of the link
    /// </summary>
    public string Label { get; }

    /// <inheritdoc />
    public override string MetadataFieldName => "URL";

    /// <inheritdoc />
    public override string MetadataFieldValue => $"[{Label}]({DocumentationURL})";
}
