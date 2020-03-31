using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Asserts that the SubProcess will return true.
    /// </summary>
    public class AssertTrue : AssertBool
    {
        /// <inheritdoc />
        protected override bool GetExpectedResult() => true;
    }

    /// <summary>
    /// Asserts that the SubProcess will return false.
    /// </summary>
    public class AssertFalse : AssertBool
    {
        /// <inheritdoc />
        protected override bool GetExpectedResult() => false;
    }

    /// <summary>
    /// Asserts that the SubProcess will return a particular value.
    /// </summary>
    public abstract class AssertBool : Process
    {
        /// <summary>
        /// The process whose result should be checked.
        /// Should have a return type of bool.
        /// </summary>
        [YamlMember(Order = 4 )]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Process SubProcess { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// The expected result of the SubProcess.
        /// </summary>
        /// <returns></returns>
        protected abstract bool GetExpectedResult();

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetAssertBoolProcessName(SubProcess?.GetName() ?? "", GetExpectedResult());

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var frozenProcess =
                SubProcess?.TryFreeze(processSettings)??Result.Failure<ImmutableProcess, ErrorList>(new ErrorList($"'{nameof(SubProcess)}' must be set."));

            if (frozenProcess.IsFailure)
                return frozenProcess;

            if (frozenProcess.Value is ImmutableProcess<bool> icp)
                return Result.Success<ImmutableProcess, ErrorList>(new immutable.AssertBool( icp, GetExpectedResult()));

            return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(
                $"'{nameof(SubProcess)}' must have return type 'bool'."));

        }
    }
}