namespace Sequence.Core.Internal;

/// <summary>
/// The name of an entity property
/// </summary>
public record EntityNestedKey
{
    /// <summary>
    /// Create a new EntityPropertyKey
    /// </summary>
    public EntityNestedKey(IEnumerable<EntityKey> values) => KeyNames = values.ToArray();

    /// <summary>
    /// Create a new EntityPropertyKey
    /// </summary>
    public EntityNestedKey(params EntityKey[] values) => KeyNames = values;

    /// <summary>
    /// Create a new EntityPropertyKey
    /// </summary>
    public EntityNestedKey(params string[] values) =>
        KeyNames = values.Select(x => new EntityKey(x)).ToArray();

    /// <summary>
    /// Create a new EntityPropertyKey
    /// </summary>
    public static EntityNestedKey Create(string s)
    {
        return new EntityNestedKey(
            s.Split(
                    Separator,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                )
                .Select(x => new EntityKey(x))
        );
    }

    /// <summary>
    /// The property Keys
    /// </summary>
    public IReadOnlyList<EntityKey> KeyNames { get; }

    /// <summary>
    /// Create an EntityPropertyKey from a string
    /// </summary>
    public EntityNestedKey(string s) : this(new EntityKey(s)) { }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (KeyNames.Count == 0)
            return 0;

        return HashCode.Combine(
            KeyNames.Count,
            KeyNames[0].GetHashCode(),
            KeyNames[^1].GetHashCode()
        );
    }

    /// <summary>
    /// Split the first key from the remainder
    /// </summary>
    public (EntityKey firstKey, Maybe<EntityNestedKey> remainder) Split()
    {
        if (KeyNames.Count == 1)
            return (KeyNames[0], Maybe<EntityNestedKey>.None);

        var remainder = new EntityNestedKey(KeyNames.Skip(1).ToList());

        return (KeyNames[0], remainder);
    }

    /// <summary>
    /// String representation of this String
    /// </summary>
    public string AsString => string.Join(Separator, KeyNames);

    /// <inheritdoc />
    public override string ToString() => AsString;

    /// <inheritdoc />
    public virtual bool Equals(EntityNestedKey? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return KeyNames.SequenceEqual(other.KeyNames);
    }

    /// <summary>
    /// The entity property key separator
    /// </summary>
    public const char Separator = '.';
}
