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
        KeyNames =
            values.SelectMany(
                    x => x.Split(
                        ".",
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    )
                )
                .ToList();
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
            KeyNames.First().ToLowerInvariant().GetHashCode(),
            KeyNames.Last().ToLowerInvariant().GetHashCode()
        );
    }

    /// <summary>
    /// Split the first key from the remainder
    /// </summary>
    public (string firstKey, Maybe<EntityPropertyKey> remainder) Split()
    {
        if (KeyNames.Count == 1)
            return (KeyNames.First(), Maybe<EntityPropertyKey>.None);

        var remainder = new EntityPropertyKey(KeyNames.Skip(1).ToList());

        return (KeyNames.First(), remainder);
    }

    /// <summary>
    /// String representation of this String
    /// </summary>
    public string AsString => string.Join(".", KeyNames);

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
}

}
