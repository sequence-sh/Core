using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.immutable.chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable.chain
{
    /// <summary>
    /// A series of processes where the result of each process is fed into the following process.
    /// </summary>
    public class Chain : Process
    {
        /// <summary>
        /// The process for this step in the immutableChain.
        /// </summary>
        [YamlMember(Order = 1)]
        [Required]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Process Process { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// The next step in the immutableChain.
        /// </summary>
        [YamlMember(Order = 2)]
        public ChainLink? Into { get; set; }

        /// <inheritdoc />
        public override string GetReturnTypeInfo()
        {
            if (Into == null)
                return Process.GetReturnTypeInfo();
            else return Into.GetReturnTypeInfo();
        }

        /// <inheritdoc />
        public override string GetName()
        {
            return ProcessNameHelper.GetChainName(Process.GetName(), Into?.GetName());
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess> TryFreeze(IProcessSettings processSettings)
        {
            var linkResult = TryCreateChainLink(processSettings);

            if (linkResult.IsFailure)
                return linkResult.ConvertFailure<ImmutableProcess>();

            return linkResult.Value;
        }

        public Result<IImmutableChainLink<TInput, TFinal>> TryCreateChainLink<TInput, TFinal>(IProcessSettings processSettings)
        {
            var chainLinkBuilder = Process.CreateChainLinkBuilder<TInput, TFinal>();


            if (Into == null)
            {
                var chainLink = chainLinkBuilder.CreateChainLink(null, processSettings, null);
                return Result.Success<IImmutableChainLink<TInput>>(chainLink);
            }
            else
            {
                var chainLink = chainLinkBuilder.CreateChainLink(Into, processSettings);
                return Result.Success<IImmutableChainLink<TInput>>(chainLink);
            }
        }


        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            return Into == null ?
                Process.GetRequirements() :
                Process.GetRequirements().Concat(Into.GetRequirements()).Distinct();
        }

    }
}