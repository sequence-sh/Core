using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A process that can be run.
    /// </summary>
    public interface IRunnableProcess
    {
        /// <summary>
        /// The name of this process.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Convert this RunnableProcess into a FreezableProcess.
        /// </summary>
        IFreezableProcess Unfreeze();

        /// <summary>
        /// Run this process and return the result untyped.
        /// </summary>
        Result<T, IRunErrors> Run<T>(ProcessState processState);

        /// <summary>
        /// Verify that this process can be run with the current set.
        /// </summary>
        Result<Unit, IRunErrors> Verify();

    }

    /// <summary>
    /// A process that can be run.
    /// </summary>
    public interface  IRunnableProcess<T> : IRunnableProcess
    {
        /// <summary>
        /// Run this process and return the result.
        /// </summary>
        Result<T, IRunErrors> Run(ProcessState processState); //TODO async
    }
}