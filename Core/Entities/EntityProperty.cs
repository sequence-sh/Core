using System;

namespace Reductech.EDR.Core.Entities;

/// <summary>
/// A property of an entity
/// </summary>
public readonly struct EntityProperty : IEquatable<EntityProperty>
{
    /// <summary>
    /// Create a new Entity property
    /// </summary>
    public EntityProperty(string name, EntityValue value, int order)
    {
        Name  = name;
        Value = value;
        Order = order;
    }

    /// <summary>
    /// The name of this property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The value of this property
    /// </summary>
    public EntityValue Value { get; }

    /// <summary>
    /// Where in the ordered list of properties this appears.
    /// </summary>
    public int Order { get; }

    /// <inheritdoc />
    public bool Equals(EntityProperty other)
    {
        var r = Name == other.Name
             && Value.Equals(other.Value)
             && Order == other.Order;

        return r;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EntityProperty other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Name, Value, Order);

    /// <inheritdoc />
    public override string ToString() => $"{Name}: {Value}";

    /// <summary>
    /// Equals operator.
    /// </summary>
    public static bool operator ==(EntityProperty left, EntityProperty right) => left.Equals(right);

    /// <summary>
    /// Not Equals operator.
    /// </summary>
    public static bool operator !=(EntityProperty left, EntityProperty right) => !(left == right);
}
