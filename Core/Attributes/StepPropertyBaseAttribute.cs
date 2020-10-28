using System;

namespace Reductech.EDR.Core.Attributes
{
    /// <summary>
    /// A property that will be used by the step.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class StepPropertyBaseAttribute : Attribute
    {
        /// <summary>
        /// The order where this property should appear.
        /// </summary>
        public int Order { get; set; }
    }
}