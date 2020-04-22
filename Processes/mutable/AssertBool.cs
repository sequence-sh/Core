using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
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
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var frozenProcess =
                ResultOf?.TryFreeze(processSettings)??Result.Failure<ImmutableProcess, ErrorList>(new ErrorList($"'{nameof(ResultOf)}' must be set."));

            if (frozenProcess.IsFailure)
                return frozenProcess;

            if (frozenProcess.Value is ImmutableProcess<bool> icp)
                return Result.Success<ImmutableProcess, ErrorList>(new immutable.AssertBool( icp, GetExpectedResult()));

            return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(
                $"'{nameof(ResultOf)}' must have return type 'bool'."));

        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            if (ResultOf == null)
                return Enumerable.Empty<string>();

            return ResultOf.GetRequirements();
        }
    }
}