using System;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A step that returns a fixed value when run.
    /// </summary>
    public interface IConstantStep : IStep
    {
        /// <summary>
        /// The output type.
        /// </summary>
        Type OutputType { get; }
    }
}