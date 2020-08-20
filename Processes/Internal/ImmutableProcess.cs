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
        Result<object> RunUntyped(ProcessState processState);

    }


}
