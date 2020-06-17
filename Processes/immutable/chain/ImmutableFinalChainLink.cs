using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.mutable.chain;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable.chain
{
    /// <summary>
    /// The final link in the immutableChain.
    /// </summary>
    public class ImmutableFinalChainLink<TInput, TFinal, TProcess> : IImmutableChainLink<TInput, TFinal>
        where TProcess : ImmutableProcess<TFinal>
    {
        /// <summary>
        /// The final link in the immutableChain.
        /// </summary>
        /// <param name="processFactory"></param>
        public ImmutableFinalChainLink(ProcessFactory<TInput, TFinal, TProcess> processFactory)
        {
            ProcessFactory = processFactory;
        }

        /// <summary>
        /// Gets the process.
        /// </summary>
        public ProcessFactory<TInput, TFinal, TProcess> ProcessFactory { get; }

        /// <inheritdoc />
        public async IAsyncEnumerable<IProcessOutput<TFinal>> Execute(TInput input)
        {
            var (_, isFailure, process, errorList) = ProcessFactory.TryCreate(input);

            if (isFailure)
                foreach (var errorLine in errorList)
                    yield return ProcessOutput<TFinal>.Error(errorLine);
            else
                await foreach (var line in process.Execute())
                    yield return line;
        }

        /// <inheritdoc />
        public string Name => ProcessFactory.Name;
    }
}