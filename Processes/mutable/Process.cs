using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.chain;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Base class of all processes.
    /// </summary>
    public abstract class Process
    {
        /// <summary>
        /// The type of this process, or a description of how the type is calculated.
        /// </summary>
        public abstract string GetReturnTypeInfo();

        /// <summary>
        /// The name of this process.
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// Executes this process. Should only be called if all conditions are met.
        /// </summary>
        /// <returns></returns>
        public abstract Result<ImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings);

        /// <summary>
        /// Gets special requirements for the process.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetRequirements();

        /// <summary>
        /// Creates a immutableChain link builder.
        /// </summary>
        public abstract Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>();

        /// <inheritdoc />
        public override string ToString()
        {
            return GetName();
        }

        /// <summary>
        /// Converts the result of freezing to the appropriate type.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TActual"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        protected Result<ImmutableProcess<TOutput>> TryConvertFreezeResult<TOutput, TActual>(Result<ImmutableProcess<TActual>> result)
        {
            if (result.IsFailure) return result.ConvertFailure<ImmutableProcess<TOutput>>();

            if (result.Value is ImmutableProcess<TOutput> process) return process;

            return Result.Failure<ImmutableProcess<TOutput>>($"{GetName()} has output type: '{typeof(TActual).Name}', not '{typeof(TOutput).Name}'.");
        }

    }
}