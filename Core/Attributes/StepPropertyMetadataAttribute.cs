namespace Reductech.EDR.Core.Attributes;

/// <summary>
/// Indicates Step Property Metadata as it will appear in the documentation
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public abstract class StepPropertyMetadataAttribute : Attribute
{
    /// <summary>
    /// The name of the metadata field
    /// </summary>
    public abstract string MetadataFieldName { get; }

    /// <summary>
    /// The value of the metadata field
    /// </summary>
    public abstract string MetadataFieldValue { get; }
}
