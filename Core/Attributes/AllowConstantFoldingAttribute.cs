namespace Sequence.Core.Attributes;

/// <summary>
/// Indicates that a step allows constant folding
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AllowConstantFoldingAttribute : Attribute { }
