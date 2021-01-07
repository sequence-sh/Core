using System;

namespace Reductech.EDR.Core.Entities
{

/// <summary>
/// A property of an entity
/// </summary>
public readonly struct EntityProperty : IEquatable<EntityProperty>
{
    /// <summary>
    /// Create a new Entity property
    /// </summary>
    public EntityProperty(string name, EntityValue baseValue, EntityValue? newValue, int order)
    {
        Name      = name;
        BaseValue = baseValue;
        NewValue  = newValue;
        Order     = order;
    }

    /// <summary>
    /// The name of this property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The base value of this property.
    /// This will be the original value (unless it has been changed).
    /// </summary>
    public EntityValue BaseValue { get; }

    /// <summary>
    /// The value of this property after schema application.
    /// </summary>
    public EntityValue? NewValue { get; }

    /// <summary>
    /// Where in the ordered list of properties this appears.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// The NewValue, if present, otherwise the BaseValue
    /// </summary>
    public EntityValue BestValue => NewValue ?? BaseValue;

    /// <inheritdoc />
    public bool Equals(EntityProperty other)
    {
        return Name == other.Name
            && BestValue.Equals(other.BestValue)
            && Order == other.Order;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EntityProperty other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Name, BestValue, Order);

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name}: {BestValue}";
    }
}

}
