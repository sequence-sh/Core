namespace Sequence.Core.Internal;

/// <summary>
/// The name of a variable that can be written and read from the step state.
/// </summary>
public readonly record struct VariableName(string Name)
{
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
    public override int GetHashCode() => Name.ToLowerInvariant().GetHashCode();

    /// <summary>
    /// Creates the name of a generic type argument.
    /// </summary>
    public VariableName CreateChild(int argNumber) => new(Name + "ARG" + argNumber);

    /// <inheritdoc />
    public override string ToString() => Serialize(SerializeOptions.Name);

    /// <summary>
    /// Serialize this VariableName
    /// </summary>
    /// <returns></returns>
    public string Serialize(SerializeOptions options) => $"<{Name}>";

    /// <summary>
    /// Automatic variable name
    /// </summary>
    public static VariableName Item { get; } = new("item");
}
