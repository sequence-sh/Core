namespace Reductech.EDR.Core.Attributes;

/// <summary>
/// Indicates that this is parameter should not be used by SCL
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotAParameterAttribute : Attribute { }
