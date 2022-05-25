namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// A string format
/// </summary>
public abstract record StringFormat : NodeData<StringFormat>
{
    /// <summary>
    /// Create a String Format
    /// </summary>
    public static StringFormat Create(string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return AnyStringFormat.Instance;

        return format.ToLowerInvariant() switch
        {
            "date-time" => DateTimeStringFormat.Instance,
            _           => AnyStringFormat.Instance
        };
    }

    /// <summary>
    /// Try to transform the object to match this format
    /// </summary>
    public abstract Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform(
        string propertyName,
        ISCLObject entityValue,
        TransformSettings transformSettings);

    /// <inheritdoc />
    public override StringFormat Combine(StringFormat other)
    {
        if (this == other)
            return this;

        return AnyStringFormat.Instance;
    }

    /// <summary>
    /// Are the allowed values a superset (not strict) of the allowed values of the other node.
    /// </summary>
    public abstract bool IsSuperset(StringFormat other);

    /// <summary>
    /// Get the type reference
    /// </summary>
    public abstract TypeReference GetTypeReference(StringRestrictions restrictions);
}
