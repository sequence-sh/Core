using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// Returns a given boolean. Useful for testing.
    /// </summary>
    public class ReturnBool : Process
    {
        /// <summary>
        /// The boolean value to return.
        /// </summary>
        [YamlMember]
        [Required]
        public bool ResultBool { get; set; }

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Boolean);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetReturnBoolProcessName(ResultBool);

        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var r = new Immutable.ReturnBool(ResultBool);

            return TryConvertFreezeResult<TOutput, bool>(r);
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput, bool, TFinal, Immutable.ReturnBool, ReturnBool>(this);
        }
    }
}