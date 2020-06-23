using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable.chain
{
    /// <summary>
    /// An immutable immutableChain.
    /// </summary>
    /// <typeparam name="TFinal"></typeparam>
    public class ImmutableChainProcess<TFinal> : ImmutableProcess<TFinal>
    {
        /// <summary>
        /// Creates a new immutableChain process.
        /// </summary>
        /// <param name="immutableChain"></param>
        public ImmutableChainProcess(IImmutableChainLink<Unit, TFinal> immutableChain)
        {
            ImmutableChain = immutableChain;
        }

        /// <inheritdoc />
        public override string Name => ImmutableChain.Name;

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;

        /// <inheritdoc />
        public override IAsyncEnumerable<IProcessOutput<TFinal>> Execute()
        {
            return ImmutableChain.Execute(Unit.Instance);
        }

        /// <summary>
        /// The immutableChain to execute.
        /// </summary>
        public IImmutableChainLink<Unit, TFinal> ImmutableChain { get; }
    }
}