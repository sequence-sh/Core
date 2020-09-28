using System.Collections.Generic;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// One or more errors thrown by a running step.
    /// </summary>
    public interface IRunErrors : IRunErrorBase
    {
        /// <summary>
        /// The errors.
        /// </summary>
        IEnumerable<IRunError> AllErrors { get; }
    }
}