using System;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Optional attribute allowing you to assign an alias to a process.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class YamlProcessAttribute : Attribute
    {
        public string Alias { get; set; }
    }
}