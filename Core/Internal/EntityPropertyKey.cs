using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// The name of an entity property
/// </summary>
public record EntityPropertyKey
{
    /// <summary>
    /// Create a new EntityPropertyKey
    /// </summary>
    public EntityPropertyKey(IEnumerable<string> values)
    {
        KeyNames = values.ToList();
    }

    /// <summary>
    /// Create a new EntityPropertyKey
    /// </summary>
    public EntityPropertyKey(params string[] values) : this(values.AsEnumerable()) { }

    /// <summary>
    /// Create a new EntityPropertyKey
    /// </summary>
    public static EntityPropertyKey Create(string s)
    {
        return new EntityPropertyKey(
            s.Split(
                Separator,
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
            )
        );
    }

    /// <summary>
    /// The property Keys
    /// </summary>
    public IReadOnlyList<string> KeyNames { get; }

    /// <summary>
    /// Create an EntityPropertyKey from a string
    /// </summary>
    public EntityPropertyKey(string s) : this(new List<string> { s }) { }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (KeyNames.Count == 0)
            return 0;

        return HashCode.Combine(
            KeyNames.Count,
            KeyNames[0].ToLowerInvariant().GetHashCode(),
            KeyNames[^1].ToLowerInvariant().GetHashCode()
        );
    }

    /// <summary>
    /// Split the first key from the remainder
    /// </summary>
    public (string firstKey, Maybe<EntityPropertyKey> remainder) Split()
    {
        if (KeyNames.Count == 1)
            return (KeyNames[0], Maybe<EntityPropertyKey>.None);

        var remainder = new EntityPropertyKey(KeyNames.Skip(1).ToList());

        return (KeyNames[0], remainder);
    }

    /// <summary>
    /// String representation of this String
    /// </summary>
    public string AsString => string.Join(Separator, KeyNames);

    /// <inheritdoc />
    public override string ToString() => AsString;

    /// <inheritdoc />
    public virtual bool Equals(EntityPropertyKey? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return KeyNames.SequenceEqual(other.KeyNames, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The entity property key separator
    /// </summary>
    public const char Separator = '.';
}

}
