namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A reference to a step property.
/// Either the name of the property or the parameter order
/// </summary>
public abstract record StepParameterReference
{
    /// <summary>
    /// A reference by parameter order
    /// </summary>
    public record Index(int I) : StepParameterReference
    {
        /// <inheritdoc />
        public override string Name => $"Parameter {I}";
    }

    /// <summary>
    /// A reference by parameter name
    /// </summary>
    public record Named(string ParameterName) : StepParameterReference
    {
        /// <inheritdoc />
        public override string Name => ParameterName;

        /// <inheritdoc />
        public virtual bool Equals(Named? other)
        {
            if (other is null)
                return false;

            return ParameterName.Equals(other.ParameterName, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <summary>
    /// This reference, in human readable form
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets possible StepParameterReferences for this property.
    /// </summary>
    public static IEnumerable<StepParameterReference> GetPossibleReferences(MemberInfo propertyInfo)
    {
        var attribute = propertyInfo.GetCustomAttribute<StepPropertyBaseAttribute>();

        if (attribute == null)
            yield break;

        yield return new Named(propertyInfo.Name);

        if (attribute.Order.HasValue)
            yield return new Index(attribute.Order.Value);

        foreach (var alias in propertyInfo.GetCustomAttributes<AliasAttribute>())
            yield return new Named(alias.Name);
    }
}
