namespace Sequence.Core.Attributes;

/// <summary>
/// Indicates that this is a configurable property of the step.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class StepPropertyAttribute : StepPropertyBaseAttribute
{
    /// <inheritdoc />
    public StepPropertyAttribute(int order) : base(order) { }

    /// <inheritdoc />
    public StepPropertyAttribute() { }

    /// <inheritdoc />
    public override MemberType MemberType => MemberType.Step;
}
