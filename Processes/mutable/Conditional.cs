using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Runs the 'If' process. If it completed successfully then run the 'Then' process, otherwise run the 'Else' process.
    /// </summary>
    public class Conditional : Process
    {
        /// <summary>
        /// The process to use as the assertion.
        /// Must have the boolean result type.
        /// </summary>
        [Required]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Process If { get; set; }

        /// <summary>
        /// If the 'If' process was successful then run this.
        /// Must have the same result type as the 'Else' process, if there is one and the void type otherwise.
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
        public Process Then { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// If the 'If' process was unsuccessful then run this.
        /// Must have the same result type as the 'Then' process.
        /// </summary>
        [YamlMember(Order = 3)]
        public Process? Else { get; set; }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var ifResult = If?.TryFreeze(processSettings)?? Result.Failure<ImmutableProcess, ErrorList>(new ErrorList($"'{nameof(If)}' must be set."));
            var thenResult = Then?.TryFreeze(processSettings)?? Result.Failure<ImmutableProcess, ErrorList>(new ErrorList($"'{nameof(Then)}' must be set."));


            var elseResult = Else?.TryFreeze(processSettings) ?? Result.Success<ImmutableProcess, ErrorList>(DoNothing.Instance);

            var combinedResult = Result.Combine(ErrorList.Compose, ifResult, thenResult, elseResult);

            if (combinedResult.IsFailure) return combinedResult.ConvertFailure<ImmutableProcess>();

            var result = CreateImmutableProcess(ifResult.Value, thenResult.Value, elseResult.Value);

            return result;
        }

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => "Returns the same type as the 'Then' and 'Else' processes. Returns void if there is no Else process.";

        /// <inheritdoc />
        public override string GetName() =>
            ProcessNameHelper.GetConditionalName(If.GetName(), Then.GetName(), Else?.GetName());



        private static Result<ImmutableProcess, ErrorList> CreateImmutableProcess(ImmutableProcess @if,
            ImmutableProcess then, ImmutableProcess @else)
        {
            var errors = new ErrorList();
            if (then.ResultType != @else.ResultType)
                errors.Add($"Then and Else should have the same type, but their types are '{then.ResultType}' and '{@else.ResultType}'");

            if (@if is ImmutableProcess<bool> ifProcess)
            {
                if(errors.Any())
                    return Result.Failure<ImmutableProcess, ErrorList>(errors);

                ImmutableProcess ip;

                try
                {
                    ip = CreateImmutableConditional(ifProcess, then as dynamic, @else as dynamic);
                }
                catch (Exception e)
                {
                    errors.Add(e.Message);
                    return Result.Failure<ImmutableProcess, ErrorList>(errors);
                }

                return Result.Success<ImmutableProcess, ErrorList>(ip);
            }
            else
            {
                errors.Add($"If process should have type bool");
                return Result.Failure<ImmutableProcess, ErrorList>(errors);
            }
        }

        private static ImmutableProcess CreateImmutableConditional<T>(ImmutableProcess<bool> ifP, ImmutableProcess<T> thenP, ImmutableProcess<T> elseP)
        {
            return new Conditional<T>(ifP, thenP, elseP);
        }

    }
}