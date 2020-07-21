using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// Delays for a given amount of time.
    /// </summary>
    public class Delay : Process
    {
        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <summary>
        /// The number of milliseconds to delay
        /// </summary>
        [YamlMember]
        [Required]
        public int Milliseconds { get; set; }


        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetDelayProcessName(Milliseconds);

        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var r = new Immutable.Delay(Milliseconds);

            return TryConvertFreezeResult<TOutput, Unit>(r);
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput, Unit, TFinal, Immutable.Delay, Delay>(this);
        }
    }
}