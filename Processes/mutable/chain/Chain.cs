using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Immutable.Chain;
using Reductech.EDR.Processes.Mutable.Injections;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable.Chain
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
            {
                if (Process == null)
                    return "The same as the type of the final process in the chain.";
                else return Process.GetReturnTypeInfo();
            }
            else return Into.GetReturnTypeInfo();
        }

        /// <inheritdoc />
        public override string GetName()
        {
            return ProcessNameHelper.GetChainName(Process.GetName(), Into?.GetName());
        }

        /// <inheritdoc />
        public override Result<IImmutableProcess<TFinal>> TryFreeze<TFinal>(IProcessSettings processSettings)
        {
            if (Process == null)
                return Result.Failure<IImmutableProcess<TFinal>>($"{nameof(Process)} must not be null");


            var linkResult = Process.TryCreateChainLinkBuilder<Unit, TFinal>().Bind(clb=> clb.CreateFirstChainLink(Into, processSettings));

            if (linkResult.IsFailure)
                return linkResult.ConvertFailure<IImmutableProcess<TFinal>>();

            var process = new ImmutableChainProcess<TFinal>(linkResult.Value);

            return process;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetAllRequirements()
        {
            IEnumerable<string> additionalRequirements;

            if (Process == null) additionalRequirements = Enumerable.Empty<string>();
            else
            {
                additionalRequirements = Into == null ?
                    Process.GetAllRequirements() :
                    Process.GetAllRequirements().Concat(Into.GetAllRequirements()).Distinct();
            }

            return base.GetAllRequirements().Concat(additionalRequirements).Distinct();
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