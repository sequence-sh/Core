namespace Sequence.Core.Attributes;

/// <summary>
/// Indicates the value to be used as a delimiter
/// </summary>
public sealed class ValueDelimiterAttribute : StepPropertyMetadataAttribute
{
    /// <summary>
    /// Create a new ValueDelimiterAttribute
    /// </summary>
    /// <param name="delimiter"></param>
    public ValueDelimiterAttribute(string delimiter)
    {
        Delimiter = delimiter;
    }

    /// <summary>
    /// The delimiter.
    /// </summary>
    public string Delimiter { get; }

    /// <inheritdoc />
    public override string MetadataFieldName => "Value Delimiter";

    /// <inheritdoc />
    public override string MetadataFieldValue => Delimiter;
}
