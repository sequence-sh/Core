using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Mutable.Chain;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable.Chain
{
    /// <summary>
    /// A link in a immutableChain.
    /// </summary>
    public class ImmutableChainLink<TInput, TOutput, TFinal, TImmutableProcess> : IImmutableChainLink<TInput, TFinal>
        where TImmutableProcess : IImmutableProcess<TOutput>
    {
        /// <summary>
        /// Creates a new ImmutableChainLink
        /// </summary>
        public ImmutableChainLink(ProcessFactory<TInput, TOutput, TImmutableProcess> processFactory, IImmutableChainLink<TOutput, TFinal> nextLink)
        {
            ProcessFactory = processFactory;
            NextLink = nextLink;
        }

        /// <summary>
        /// Gets the process.
        /// </summary>
        public ProcessFactory<TInput, TOutput, TImmutableProcess> ProcessFactory { get; }

        /// <summary>
        /// The next link in the immutableChain.
        /// </summary>
        public IImmutableChainLink<TOutput, TFinal> NextLink { get; }

        /// <inheritdoc />
        public string Name => ProcessNameHelper.GetChainName(ProcessFactory.Name, NextLink.Name);

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IProcessOutput<TFinal>> Execute(TInput input)
        {
            var (_, isFailure, process, error) = ProcessFactory.TryCreate(input);

            if (isFailure)
                yield return ProcessOutput<TFinal>.Error(error);
            else
            {
                var failed = false;
                IProcessOutput<TOutput>? processOutput = null;

                await foreach (var output in process.Execute())
                {
                    switch (output.OutputType)
                    {
                        case OutputType.Error: failed = true; yield return output.ConvertTo<TFinal>(); break;
                        case OutputType.Warning: yield return output.ConvertTo<TFinal>(); break;
                        case OutputType.Message: yield return output.ConvertTo<TFinal>(); break;
                        case OutputType.Success: processOutput = output;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if(failed) yield break;
                if (processOutput == null)
                    yield return ProcessOutput<TFinal>.Error($"'{Name}' did not return success.");
                else
                    await foreach (var nextOutput in NextLink.Execute(processOutput.Value))
                        yield return nextOutput;
            }
        }
    }
}