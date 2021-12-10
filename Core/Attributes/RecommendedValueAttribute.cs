namespace Reductech.EDR.Core.Attributes;

/// <summary>
/// The recommended value for this parameter.
/// </summary>
public sealed class RecommendedValueAttribute : StepPropertyMetadataAttribute
{
    /// <summary>
    /// Creates a new RecommendedValueAttribute.
    /// </summary>
    /// <param name="recommendedValue"></param>
    public RecommendedValueAttribute(string recommendedValue)
    {
        RecommendedValue = recommendedValue;
    }

    /// <summary>
    /// The recommended value.
    /// </summary>
    public string RecommendedValue { get; }

    /// <inheritdoc />
    public override string MetadataFieldName => "Recommended Value";

    /// <inheritdoc />
    public override string MetadataFieldValue => RecommendedValue;
}
