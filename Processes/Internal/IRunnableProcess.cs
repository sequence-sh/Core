using System.Collections.Generic;
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
        /// Verify that this process can be run with the current settings.
        /// </summary>
        public Result<Unit, IRunErrors> Verify(IProcessSettings settings);

        /// <summary>
        /// Configuration for this process.
        /// </summary>
        ProcessConfiguration? ProcessConfiguration { get; set; }

        /// <summary>
        /// Process combiners that could be used for this process.
        /// </summary>
        IEnumerable<IProcessCombiner> ProcessCombiners { get; }

    }

    /// <summary>
    /// A process that can be run.
    /// </summary>
    public interface IRunnableProcess<T> : IRunnableProcess
    {
        /// <summary>
        /// Run this process and return the result.
        /// </summary>
        Result<T, IRunErrors> Run(ProcessState processState); //TODO async
    }
}