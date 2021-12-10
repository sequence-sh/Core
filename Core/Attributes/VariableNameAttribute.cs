namespace Reductech.EDR.Core.Attributes;

/// <summary>
/// Indicates that this property is the name of a variable.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class VariableNameAttribute : StepPropertyBaseAttribute
{
    /// <inheritdoc />
    public VariableNameAttribute(int order) : base(order) { }

    /// <inheritdoc />
    public VariableNameAttribute() { }

    /// <inheritdoc />
    public override MemberType MemberType => MemberType.VariableName;
}
