using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A process that can be run.
    /// </summary>
    public interface  IRunnableProcess<T> : IRunnableProcess
    {
        /// <summary>
        /// Run this process and return the result.
        /// </summary>
        Result<T> Run(ProcessState processState); //TODO async
    }
}