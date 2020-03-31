using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Asserts that the count of the SubProcess is within a particular range.
    /// </summary>
    public class AssertCount : Process
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
        public Process SubProcess { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetAssertCountProcessName(SubProcess?.GetName() ?? "");

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            if (Minimum == null || Maximum == null)
                return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList($"Either {nameof(Minimum)} or {nameof(Maximum)} must be set."));

            var frozenCount =
                SubProcess?.TryFreeze(processSettings)??Result.Failure<ImmutableProcess, ErrorList>(new ErrorList($"'{nameof(SubProcess)}' must be set."));

            if (frozenCount.IsFailure)
                return frozenCount;

            if (frozenCount.Value is ImmutableProcess<int> icp)
                return Result.Success<ImmutableProcess, ErrorList>(new immutable.AssertCount(Minimum, Maximum, icp));

            return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(
                $"'{nameof(SubProcess)}' must have return type 'int'."));

        }
    }
}