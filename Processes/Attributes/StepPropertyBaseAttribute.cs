using System;

namespace Reductech.EDR.Processes.Attributes
{
    /// <summary>
    /// A property that will be used by the step.
    /// </summary>
    public abstract class StepPropertyBaseAttribute : Attribute
    {
        /// <summary>
        /// The order where this property should appear.
        /// </summary>
        public int Order { get; set; }
    }
}