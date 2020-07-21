using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// Outputs a given message. Returns a Unit. Used for logging.
    /// </summary>
    public class OutputMessage : Process
    {
        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <summary>
        /// The message to output.
        /// </summary>
        [YamlMember]
        [Required]
#pragma warning disable 8618
        public string Message { get; set; }
#pragma warning restore 8618


        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetOutputMessageProcessName(Message);

        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var r = new Immutable.OutputMessage(Message);

            return TryConvertFreezeResult<TOutput, Unit>(r);
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput, Unit, TFinal, Immutable.OutputMessage, OutputMessage>(this);
        }
    }
}