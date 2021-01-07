using System;

namespace Reductech.EDR.Core.Attributes
{

/// <summary>
/// Indicated that this property is a function that will run in a nested scope.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ScopedFunctionAttribute : Attribute { }

}
