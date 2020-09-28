using System;

namespace Reductech.EDR.Processes.Attributes
{
    /// <summary>
    /// Indicates that this property is the name of a variable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class VariableNameAttribute : StepPropertyBaseAttribute { }
}