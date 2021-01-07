using System;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// The name of a variable that can be written and read from the step state.
/// </summary>
public readonly struct VariableName : IEquatable<VariableName>
{
    /// <summary>
    /// Creates a new Variable.
    /// </summary>
    public VariableName(string name) => Name = name;

    /// <summary>
    /// The name of the variable.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc />
    public bool Equals(VariableName other)
    {
        // ReSharper disable ConditionIsAlwaysTrueOrFalse
        if (Name == null)
            return other.Name == null;

        if (other.Name == null)
            return false;
        // ReSharper restore ConditionIsAlwaysTrueOrFalse

        return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is VariableName other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Name.ToLowerInvariant().GetHashCode();

    /// <summary>
    /// Equals operator
    /// </summary>
    public static bool operator ==(VariableName left, VariableName right) => left.Equals(right);

    /// <summary>
    /// Not Equals Operator
    /// </summary>
    public static bool operator !=(VariableName left, VariableName right) => !left.Equals(right);

    /// <summary>
    /// Creates the name of a generic type argument.
    /// </summary>
    public VariableName CreateChild(int argNumber) => new(Name + "ARG" + argNumber);

    /// <inheritdoc />
    public override string ToString() => Serialize();

    /// <summary>
    /// SerializeAsync this VariableName
    /// </summary>
    /// <returns></returns>
    public string Serialize() => $"<{Name}>";

    /// <summary>
    /// The variable that entities will be set to.
    /// </summary>

    public static VariableName Entity { get; } = new("Entity");

    /// <summary>
    /// The variable name that indexes in for loops will be set to
    /// </summary>
    public static VariableName Index { get; } = new("i");
}

}
