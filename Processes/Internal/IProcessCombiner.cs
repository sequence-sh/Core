using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// An object which can combine a process with the next process in the sequence.
    /// </summary>
    public interface IProcessCombiner
    {
        /// <summary>
        /// Tries to combine this process with the next process in the sequence.
        /// </summary>
        public Result<IRunnableProcess<Unit>> TryCombine(IRunnableProcess<Unit> p1, IRunnableProcess<Unit> p2);

    }
}
