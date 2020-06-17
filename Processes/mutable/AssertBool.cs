using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Asserts that the Check will return true.
    /// </summary>
    public class AssertTrue : AssertBool
    {
        /// <inheritdoc />
        protected override bool GetExpectedResult() => true;
    }

    /// <summary>
    /// Asserts that the Check will return false.
    /// </summary>
    public class AssertFalse : AssertBool
    {
        /// <inheritdoc />
        protected override bool GetExpectedResult() => false;

    }

    /// <summary>
    /// Asserts that the Check will return a particular value.
    /// </summary>
    public abstract class AssertBool : Process
    {
        /// <summary>
        /// The process whose result should be checked.
        /// Should have a return type of bool.
        /// </summary>
        [YamlMember(Order = 4 )]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Process ResultOf { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// The expected result of the Check.
        /// </summary>
        /// <returns></returns>
        protected abstract bool GetExpectedResult();

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetAssertBoolProcessName(ResultOf?.GetName() ?? "", GetExpectedResult());

        /// <inheritdoc />
        public override Result<ImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var r = TryFreeze(processSettings);

            return TryConvertFreezeResult<TOutput, Unit>(r);

        }

        private Result<ImmutableProcess<Unit>> TryFreeze(IProcessSettings processSettings)
        {
            var frozenProcess =
                ResultOf?.TryFreeze<bool>(processSettings)??Result.Failure<ImmutableProcess<bool>>($"'{nameof(ResultOf)}' must be set.");

            if (frozenProcess.IsFailure)
                return frozenProcess.ConvertFailure<ImmutableProcess<Unit>>();

            return new immutable.AssertBool( frozenProcess.Value, GetExpectedResult());
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            if (ResultOf == null)
                return Enumerable.Empty<string>();

            return ResultOf.GetRequirements();
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput,Unit,TFinal,immutable.AssertBool,AssertBool>(this);
        }
    }
}