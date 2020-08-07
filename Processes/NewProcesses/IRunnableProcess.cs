using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// A process that can be run.
    /// </summary>
    public interface IRunnableProcess<T> : IRunnableProcess //TODO async
    {
        /// <summary>
        /// Run this process and return the result.
        /// </summary>
        Result<T> Run(ProcessState processState);
    }
}