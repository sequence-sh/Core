namespace Sequence.Core.Attributes;

/// <summary>
/// Allows users to set custom metadata for a Step Property
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
public sealed class MetadataAttribute : Attribute
{
    /// <inheritdoc />
    public MetadataAttribute(string key, string value)
    {
        Key   = key;
        Value = value;
    }

    /// <summary>
    /// Generally the Category of the Metadata
    /// </summary>
    public string Key { get; init; }

    /// <summary>
    /// A more specific Value
    /// </summary>
    public string Value { get; init; }
}
