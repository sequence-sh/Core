namespace Sequence.Core.Attributes;

/// <summary>
/// Indicates that a Step should not be included in a connector's steps
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class NotAStaticStepAttribute : Attribute { }
