using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Mutable.Chain;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable.Chain
{
    /// <summary>
    /// The final link in the immutableChain.
    /// </summary>
    public class ImmutableFinalChainLink<TInput, TFinal, TImmutableProcess> :
        IImmutableChainLink<TInput, TFinal>
        where TImmutableProcess : IImmutableProcess<TFinal>
    {
        /// <summary>
        /// The final link in the immutableChain.
        /// </summary>
        /// <param name="processFactory"></param>
        public ImmutableFinalChainLink(ProcessFactory<TInput, TFinal, TImmutableProcess> processFactory)
        {
            ProcessFactory = processFactory;
        }

        /// <summary>
        /// Gets the process.
        /// </summary>
        public ProcessFactory<TInput, TFinal, TImmutableProcess> ProcessFactory { get; }

        /// <inheritdoc />
        public async IAsyncEnumerable<IProcessOutput<TFinal>> Execute(TInput input)
        {
            var (_, isFailure, immutableProcess, error) = ProcessFactory.TryCreate(input);

            if (isFailure)
                yield return ProcessOutput<TFinal>.Error(error);
            else
                await foreach (var line in immutableProcess.Execute())
                    yield return line;
        }

        /// <inheritdoc />
        public string Name => ProcessFactory.Name;
    }
}