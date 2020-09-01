using System.Collections.Generic;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A runnable process that is not a constant.
    /// </summary>
    public interface ICompoundRunnableProcess : IRunnableProcess
    {
        /// <summary>
        /// The factory used to create processes of this type.
        /// </summary>
        IRunnableProcessFactory RunnableProcessFactory { get; }

        /// <summary>
        /// Requirements for this process that can only be determined at runtime.
        /// </summary>
        IEnumerable<Requirement> RuntimeRequirements { get; }
    }

    /// <summary>
    /// A runnable process that is not a constant.
    /// </summary>
    public interface ICompoundRunnableProcess<T> : IRunnableProcess<T>, ICompoundRunnableProcess
    {
    }
}