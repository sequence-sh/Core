using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.immutable.chain;
using Reductech.EDR.Utilities.Processes.mutable.injection;
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
        public override Result<ImmutableProcess<TFinal>> TryFreeze<TFinal>(IProcessSettings processSettings)
        {
            var linkResult = Process.TryCreateChainLinkBuilder<Unit, TFinal>().Bind(clb=> clb.CreateFirstChainLink(Into, processSettings));

            if (linkResult.IsFailure)
                return linkResult.ConvertFailure<ImmutableProcess<TFinal>>();

            var process = new ImmutableChainProcess<TFinal>(linkResult.Value);

            return process;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            return Into == null ?
                Process.GetRequirements() :
                Process.GetRequirements().Concat(Into.GetRequirements()).Distinct();
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return Result.Failure<ChainLinkBuilder<TInput, TFinal>>("Cannot nest a chain within a chain");
        }
    }


    /// <summary>
    /// A step in the immutableChain other than the first.
    /// </summary>
    public class ChainLink : Chain
    {
        /// <summary>
        /// The injection to inject the result of the previous method.
        /// </summary>
        [YamlMember(Order = 3)]
        [Required]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Injection Inject { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// Creates a link in a chain.
        /// </summary>
        public Result<IImmutableChainLink<TInput, TFinal>> TryCreateChainLink<TInput, TFinal>(IProcessSettings processSettings)
        {
            if (Inject == null)
                return Result.Failure<IImmutableChainLink<TInput, TFinal>>($"{nameof(Inject)} must be set.");

            var chainLinkBuilder = Process.TryCreateChainLinkBuilder<TInput, TFinal>();
            var chainLink = chainLinkBuilder.Bind(clb=> clb.CreateChainLink(Into, processSettings, Inject)) ;

            return chainLink;
        }
    }
}