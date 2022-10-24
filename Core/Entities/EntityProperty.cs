namespace Reductech.Sequence.Core.Entities;

/// <summary>
/// A property of an entity
/// </summary>
public readonly record struct EntityProperty
    (string Name, ISCLObject Value, int Order) // TODO remove
{
    /// <inheritdoc />
    public override string ToString() => $"{Name}: {Value}";
}
