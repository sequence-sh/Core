namespace Reductech.EDR.Core.Attributes
{

/// <summary>
/// Indicates the recommended range for this parameter.
/// </summary>
public sealed class RecommendedRangeAttribute : StepPropertyMetadataAttribute
{
    /// <summary>
    /// Creates a new RecommendedRangeAttribute.
    /// </summary>
    /// <param name="recommendedRange"></param>
    public RecommendedRangeAttribute(string recommendedRange)
    {
        RecommendedRange = recommendedRange;
    }

    /// <summary>
    /// The recommended range for this parameter.
    /// </summary>
    public string RecommendedRange { get; }

    /// <inheritdoc />
    public override string MetadataFieldName => "Recommended Range";

    /// <inheritdoc />
    public override string MetadataFieldValue => RecommendedRange;
}

}
