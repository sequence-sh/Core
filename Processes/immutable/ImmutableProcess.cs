using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// A process whose parameters can no longer be altered.
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

        //TODO make this calculated rather than set from the constructor
        /// <summary>
        /// The name of this process.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of this process' final results.
        /// </summary>
        public abstract Type ResultType { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Executes this process.
        /// </summary>
        /// <returns></returns>
        public abstract IAsyncEnumerable<IProcessOutput> ExecuteUntyped();
    }

    /// <summary>
    /// A process whose parameters can no longer be altered.
    /// </summary>
    public abstract class ImmutableProcess<T> : ImmutableProcess
    {
        /// <inheritdoc />
        protected ImmutableProcess(string name) : base(name)
        {
        }

        /// <inheritdoc />
        public override IAsyncEnumerable<IProcessOutput> ExecuteUntyped()
        {
            return Execute();
        }

        /// <summary>
        /// Executes this process.
        /// </summary>
        /// <returns></returns>
        public abstract IAsyncEnumerable<IProcessOutput<T>> Execute();

        /// <summary>
        /// Try to create a process that is this process combined with the next process.
        /// Should only work for unit processes.
        /// </summary>
        /// <param name="nextProcess"></param>
        /// <returns></returns>
        public virtual Result<ImmutableProcess<Unit>> TryCombine(ImmutableProcess<Unit> nextProcess)
        {
            return Result.Failure<ImmutableProcess<Unit>>("Could not combine");
        }

        /// <inheritdoc />
        public override Type ResultType => typeof(T);
    }
}
