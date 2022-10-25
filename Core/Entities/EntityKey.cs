namespace Reductech.Sequence.Core.Entities;

/// <summary>
/// An entity key. Case insensitive
/// </summary>
public record struct EntityKey(string Inner)
{
    /// <summary>
    /// The default property name if the EntityStruct represents a single primitive.
    /// </summary>
    public const string PrimitiveKey = "value";

    /// <summary>
    /// The primitive entity key
    /// </summary>
    public static readonly EntityKey Primitive = new("value");

    /// <inheritdoc />
    public override string ToString() => Inner;

    /// <summary>
    /// Compare with a nullable value
    /// </summary>
    public bool Equals(EntityKey? other) => other.HasValue && Equals(other.Value);

    /// <inheritdoc />
    public bool Equals(EntityKey other) => Inner.Equals(
        other.Inner,
        StringComparison.OrdinalIgnoreCase
    );

    /// <inheritdoc />
    public override int GetHashCode() => Inner.ToLowerInvariant().GetHashCode();
}
