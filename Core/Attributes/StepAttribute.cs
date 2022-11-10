namespace Sequence.Core.Attributes;

/// <summary>
/// Optional attribute allowing you to assign an alias to a step.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class StepAttribute : Attribute
{
    /// <summary>
    /// The new name for this step.
    /// </summary>
    public string? Alias { get; set; }
}
