namespace Reductech.Sequence.Core.Attributes;

/// <summary>
/// Indicates that this property is a function.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class FunctionPropertyAttribute : StepPropertyBaseAttribute
{
    /// <inheritdoc />
    public FunctionPropertyAttribute(int order) : base(order) { }

    /// <inheritdoc />
    public FunctionPropertyAttribute() { }

    /// <inheritdoc />
    public override MemberType MemberType => MemberType.Lambda;
}
