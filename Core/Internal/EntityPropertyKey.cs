using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// The name of an entity property
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public record EntityPropertyKey(IReadOnlyList<string> Values)
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <summary>
    /// Create an EntityPropertyKey from a string
    /// </summary>
    public EntityPropertyKey(string s) : this(new List<string>() { s }) { }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (Values.Count == 0)
            return 0;

        return HashCode.Combine(
            Values.Count,
            Values.First().ToLowerInvariant().GetHashCode(),
            Values.Last().ToLowerInvariant().GetHashCode()
        );
    }

    /// <summary>
    /// Split the first key from the remainder
    /// </summary>
    public (string firstKey, Maybe<EntityPropertyKey> remainder) Split()
    {
        if (Values.Count == 1)
            return (Values.First(), Maybe<EntityPropertyKey>.None);

        var remainder = new EntityPropertyKey(Values.Skip(1).ToList());

        return (Values.First(), remainder);
    }

    /// <summary>
    /// String representation of this String
    /// </summary>
    public string AsString => string.Join(".", Values);

    /// <inheritdoc />
    public override string ToString() => AsString;

    /// <inheritdoc />
    public virtual bool Equals(EntityPropertyKey? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Values.SequenceEqual(other.Values, StringComparer.OrdinalIgnoreCase);
    }
}

}
