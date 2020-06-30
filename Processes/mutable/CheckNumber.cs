using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// Checks that the count of the Check is within a particular range.
    /// </summary>
    public class CheckNumber : Process
    {
        /// <summary>
        /// Inclusive minimum of the expected range.
        /// Either this, Maximum, or both must be set.
        /// </summary>
        [YamlMember(Order = 2 )]
        public int? Minimum { get; set; }

        /// <summary>
        /// Inclusive maximum of the expected range.
        /// Either this, Minimum, or both must be set.
        /// </summary>
        [YamlMember(Order = 3 )]
        public int? Maximum { get; set; }

        /// <summary>
        /// The process whose count should be checked.
        /// Should have a return type of int.
        /// </summary>
        [YamlMember(Order = 4 )]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Process Check { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Boolean);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetCheckNumberProcessName(Check?.GetName() ?? "");


        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            return TryConvertFreezeResult<TOutput, bool>(TryFreeze(processSettings));
        }

        private Result<IImmutableProcess<bool>> TryFreeze(IProcessSettings processSettings)
        {
            if (Minimum == null && Maximum == null)
                return Result.Failure<IImmutableProcess<bool>>($"Either {nameof(Minimum)} or {nameof(Maximum)} must be set.");

            var frozenCount =
                Check?.TryFreeze<int>(processSettings)??Result.Failure<IImmutableProcess<int>>($"'{nameof(Check)}' must be set.");

            if (frozenCount.IsFailure)
                return frozenCount.ConvertFailure<IImmutableProcess<bool>>();

            return Result.Success<IImmutableProcess<bool>>(new Immutable.CheckNumber(Minimum, Maximum, frozenCount.Value));

        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            if (Check == null)
                return Enumerable.Empty<string>();

            return Check.GetRequirements();
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput,bool,TFinal,Immutable.CheckNumber,CheckNumber>(this);
        }
    }
}