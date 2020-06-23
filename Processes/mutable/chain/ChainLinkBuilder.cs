using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.immutable.chain;
using Reductech.EDR.Utilities.Processes.mutable.injection;

namespace Reductech.EDR.Utilities.Processes.mutable.chain
{
    /// <summary>
    /// Creates chain links.
    /// </summary>
    public abstract class ChainLinkBuilder<TInput, TFinal>
    {
        /// <summary>
        /// Creates a chain link.
        /// </summary>
        public abstract Result<IImmutableChainLink<TInput, TFinal>> CreateChainLink(ChainLink? next, IProcessSettings processSettings, Injection injection);

        /// <summary>
        /// Creates the first link in a chain.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="processSettings"></param>
        /// <returns></returns>
        public abstract Result<IImmutableChainLink<Unit, TFinal>> CreateFirstChainLink(ChainLink? next, IProcessSettings processSettings);
    }

    /// <summary>
    /// Creates chain links.
    /// </summary>
    public class ChainLinkBuilder<TInput, TOutput, TFinal, TImmutableProcess, TProcess> : ChainLinkBuilder<TInput, TFinal>
        where TImmutableProcess : IImmutableProcess<TOutput>
        where TProcess : Process
    {
        /// <summary>
        /// Creates a new ChainLinkBuilder.
        /// </summary>
        /// <param name="process"></param>
        public ChainLinkBuilder(TProcess process)
        {
            Process = process;
        }

        /// <summary>
        /// The process to use in this builder.
        /// </summary>
        public TProcess Process { get; }

        /// <inheritdoc />
        public override Result<IImmutableChainLink<TInput, TFinal>> CreateChainLink(ChainLink? next, IProcessSettings processSettings, Injection injection)
        {
            var processFactory = new InjectionProcessFactory<TInput,TOutput,TImmutableProcess,TProcess>(Process, processSettings, injection);

            if (next == null)
            {
                var finalChainLink = new ImmutableFinalChainLink<TInput, TOutput, TImmutableProcess>(processFactory);

                if (finalChainLink is IImmutableChainLink<TInput, TFinal> fcl)
                    return Result.Success(fcl);
                else
                    return Result.Failure<IImmutableChainLink<TInput, TFinal>>($"{Process.GetName()} does not have return type {typeof(TFinal).Name}");
            }
            else
            {
                var nextLink = next.TryCreateChainLink<TOutput, TFinal>(processSettings);
                if (nextLink.IsFailure)
                    return nextLink.ConvertFailure<IImmutableChainLink<TInput, TFinal>>();
                var thisLink = new ImmutableChainLink<TInput, TOutput, TFinal, TImmutableProcess>(processFactory, nextLink.Value);
                return thisLink;
            }
        }

        /// <inheritdoc />
        public override Result<IImmutableChainLink<Unit, TFinal>> CreateFirstChainLink(ChainLink? next, IProcessSettings processSettings)
        {
            var processFactory = new UnitProcessFactory<TOutput,TImmutableProcess, TProcess>(Process, processSettings);

            if (next == null)
            {
                var finalChainLink = new ImmutableFinalChainLink<Unit, TOutput, TImmutableProcess>(processFactory);

                if (finalChainLink is IImmutableChainLink<Unit, TFinal> fcl)
                    return Result.Success(fcl);
                else
                    return Result.Failure<IImmutableChainLink<Unit, TFinal>>($"{Process.GetName()} does not have return type {typeof(TFinal).Name}");
            }
            else
            {
                var nextLink = next.TryCreateChainLink<TOutput, TFinal>(processSettings);
                if (nextLink.IsFailure)
                    return nextLink.ConvertFailure<IImmutableChainLink<Unit, TFinal>>();
                var thisLink = new ImmutableChainLink<Unit, TOutput, TFinal, TImmutableProcess>(processFactory, nextLink.Value);
                return thisLink;
            }
        }
    }
}