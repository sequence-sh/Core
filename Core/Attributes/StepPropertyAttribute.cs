using System;

namespace Reductech.EDR.Core.Attributes
{
    /// <summary>
    /// Indicates that this is a configurable property of the step.
    /// </summary>

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class StepPropertyAttribute : StepPropertyBaseAttribute{ }

    /// <summary>
    /// Indicates that this is a serializable property of an object.
    /// </summary>

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ConfigPropertyAttribute : Attribute
    {
        /// <summary>
        /// The order where this property should appear.
        /// </summary>
        public int Order { get; set; }
    }
}
