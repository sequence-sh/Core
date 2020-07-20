﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Tests
{
    public class ReturnIntProcess : Process
    {
        [YamlMember]
        [Required]
        public int Value { get; set; }

        /// <inheritdoc />
        public override string GetReturnTypeInfo()
        {
            return typeof(int).ToString();
        }

        /// <inheritdoc />
        public override string GetName() => "Return " + Value;

        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var r = new ImmutableReturnIntProcess(Value);

            return TryConvertFreezeResult<TOutput, int>(r);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetAllRequirements()
        {
            yield break;
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput, int, TFinal, ImmutableReturnIntProcess, ReturnIntProcess>(this);
        }
    }
}