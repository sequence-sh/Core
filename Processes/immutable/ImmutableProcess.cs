using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// A process whose parameters can no longer be altered
    /// </summary>
    public abstract class ImmutableProcess
    {
        /// <summary>
        /// Create a new immutable process
        /// </summary>
        /// <param name="name"></param>
        protected ImmutableProcess(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The name of this process.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Executes this process.
        /// </summary>
        /// <returns></returns>
        public abstract IAsyncEnumerable<Result<string>> Execute();

        /// <summary>
        /// Try to create a process that is this process combined with the next process
        /// </summary>
        /// <param name="nextProcess"></param>
        /// <returns></returns>
        public virtual Result<ImmutableProcess> TryCombine(ImmutableProcess nextProcess)
        {
            return Result.Failure<ImmutableProcess>("Could not combine");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
