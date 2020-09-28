using System;

namespace Reductech.EDR.Processes.Attributes
{
    /// <summary>
    /// Indicates that this is a configurable property of the step.
    /// </summary>

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class StepPropertyAttribute : StepPropertyBaseAttribute{ }
}
