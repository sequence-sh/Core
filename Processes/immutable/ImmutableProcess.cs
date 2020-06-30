using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Mutable;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable
{
    /// <summary>
    /// A process whose parameters can no longer be altered.
    /// </summary>
    public interface IImmutableProcess<out T>
    {
        /// <summary>
        /// Executes this process.
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<IProcessOutput<T>> Execute();

        /// <summary>
        /// Try to create a process that is this process combined with the next process.
        /// Should only work for unit processes.
        /// </summary>
        Result<IImmutableProcess<Unit>> TryCombine(IImmutableProcess<Unit> nextProcess, IProcessSettings processSettings)
        {
            if (nextProcess.ProcessConverter != null)
            {
                var (isSuccess, _, value) = nextProcess.ProcessConverter.TryConvert(this, processSettings);

                if (isSuccess && this != value)
                {
                    return value.TryCombine(nextProcess, processSettings);
                }
            }

            return Result.Failure<IImmutableProcess<Unit>>("Could not combine");
        }

        /// <summary>
        /// The name of this process.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The process converter for combining with this process.
        /// </summary>
        IProcessConverter? ProcessConverter { get; }

    }


    /// <summary>
    /// A process whose parameters can no longer be altered.
    /// </summary>
    public abstract class ImmutableProcess<T> : IImmutableProcess<T>
    {
        /// <summary>
        /// Executes this process.
        /// </summary>
        /// <returns></returns>
        public abstract IAsyncEnumerable<IProcessOutput<T>> Execute();

        /// <summary>
        /// Try to create a process that is this process combined with the next process.
        /// Should only work for unit processes.
        /// </summary>
        public virtual Result<IImmutableProcess<Unit>> TryCombine(IImmutableProcess<Unit> nextProcess, IProcessSettings processSettings)
        {
            if (nextProcess.ProcessConverter != null)
            {
                var (isSuccess, _, value) = nextProcess.ProcessConverter.TryConvert(this, processSettings);

                if (isSuccess && this != value)
                {
                    return value.TryCombine(nextProcess, processSettings);
                }
            }

            return Result.Failure<IImmutableProcess<Unit>>("Could not combine");
        }

        /// <summary>
        /// The name of this process.
        /// </summary>
        public abstract string Name { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// The process converter for combining with this process.
        /// </summary>
        public abstract IProcessConverter? ProcessConverter { get; }
    }
}
