using System;

namespace Reductech.EDR.Core.Attributes
{

/// <summary>
/// Indicates that this string property should be a single character
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SingleCharacterAttribute : Attribute { }

}
