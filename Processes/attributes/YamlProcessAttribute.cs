using System;

namespace Reductech.EDR.Processes.Attributes
{
    /// <summary>
    /// Optional attribute allowing you to assign an alias to a process.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class YamlProcessAttribute : Attribute
    {
        /// <summary>
        /// The new name for this process.
        /// </summary>
        public string? Alias { get; set; }
    }
}