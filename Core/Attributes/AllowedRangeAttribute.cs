namespace Reductech.EDR.Core.Attributes;

/// <summary>
/// Indicates the allowed range of values.
/// </summary>
public sealed class AllowedRangeAttribute : StepPropertyMetadataAttribute
{
    /// <summary>
    /// Creates a new AllowedRangeAttribute.
    /// </summary>
    /// <param name="allowedRangeValue"></param>
    public AllowedRangeAttribute(string allowedRangeValue)
    {
        AllowedRangeValue = allowedRangeValue;
    }

    /// <summary>
    /// The range allowed.
    /// </summary>
    public string AllowedRangeValue { get; }

    /// <inheritdoc />
    public override string MetadataFieldName => "Allowed Range";

    /// <inheritdoc />
    public override string MetadataFieldValue => AllowedRangeValue;
}
