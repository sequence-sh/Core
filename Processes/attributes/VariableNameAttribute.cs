using System;

namespace Reductech.EDR.Processes.Attributes
{
    /// <summary>
    /// Indicates that this property is the name of a variable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class VariableNameAttribute : ProcessPropertyAttribute { }

    /// <summary>
    /// A property that will be used by the process.
    /// </summary>
    public abstract class ProcessPropertyAttribute : Attribute
    {
        /// <summary>
        /// The order where this property should appear.
        /// </summary>
        public int Order { get; set; }
    }
}