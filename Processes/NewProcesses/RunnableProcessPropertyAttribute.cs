using System;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// Indicates that this is a configurable property of the process.
    /// </summary>

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RunnableProcessPropertyAttribute : Attribute
    {
    }

    /// <summary>
    /// Indicates that this is a list of configurable properties of the process.
    /// </summary>

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RunnableProcessListPropertyAttribute : Attribute
    {

    }
}