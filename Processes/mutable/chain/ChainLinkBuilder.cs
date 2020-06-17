using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.immutable.chain;
using Reductech.EDR.Utilities.Processes.mutable.injection;

namespace Reductech.EDR.Utilities.Processes.mutable.chain
{
    /// <summary>
    /// Creates chain links.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public abstract class ChainLinkBuilder<TInput, TFinal>
    {

        /// <summary>
        /// Creates a chain link.
        /// </summary>
        public abstract IImmutableChainLink<TInput, TFinal> CreateChainLink(ChainLink? next, IProcessSettings processSettings, Injection injection);
    }

    /// <summary>
    /// Creates chain links.
    /// </summary>
    public class ChainLinkBuilder<TInput, TOutput, TFinal, TImmutableProcess, TProcess> : ChainLinkBuilder<TInput, TFinal>
        where TImmutableProcess : ImmutableProcess<TOutput>
        where TProcess : Process
    {
        public ChainLinkBuilder(TProcess process)
        {
            Process = process;
        }

        public TProcess Process { get; }

        /// <inheritdoc />
        public override IImmutableChainLink<TInput, TFinal> CreateChainLink(ChainLink? next, IProcessSettings processSettings, Injection injection)
        {
            var processFactory = new InjectionProcessFactory<TInput,TOutput,TImmutableProcess,TProcess>(Process, processSettings, injection);

            var nextLink = next.TryCreateChainLink<TOutput, TFinal>(processSettings);

            var thisLink = new ImmutableChainLink<TInput, TOutput, TFinal, TImmutableProcess>(processFactory, nextLink.Value);

            return thisLink;
        }
    }
}