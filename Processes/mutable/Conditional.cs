using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
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
        public override Result<ImmutableProcess> TryFreeze(IProcessSettings processSettings)
        {
            var ifResult = If?.TryFreeze(processSettings)?? Result.Failure<ImmutableProcess>($"'{nameof(If)}' must be set.");
            var thenResult = Then?.TryFreeze(processSettings)?? Result.Failure<ImmutableProcess>($"'{nameof(Then)}' must be set.");

            var elseResult = Else?.TryFreeze(processSettings) ?? Result.Success(DoNothing.Instance);

            var combinedResult = Result.Combine(new []{ifResult, thenResult, elseResult}, "\r\n");

            if (combinedResult.IsFailure) return combinedResult.ConvertFailure<ImmutableProcess>();

            var createResult = CreateImmutableProcess(ifResult.Value, thenResult.Value, elseResult.Value, processSettings);

            return createResult;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            if (If == null || Then == null)
                return Enumerable.Empty<string>();

            return If.GetRequirements().Concat(Then.GetRequirements())
                .Concat(Else?.GetRequirements() ?? Enumerable.Empty<string>()).Distinct();
        }

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => "Returns the same type as the 'Then' and 'Else' processes. Returns void if there is no Else process.";

        /// <inheritdoc />
        public override string GetName() =>
            ProcessNameHelper.GetConditionalName(If.GetName(), Then.GetName(), Else?.GetName());



        private static Result<ImmutableProcess> CreateImmutableProcess(ImmutableProcess @if,
            ImmutableProcess then, ImmutableProcess @else, IProcessSettings processSettings)
        {
            var errors = new StringBuilder();
            if (then.ResultType != @else.ResultType)
                errors.AppendLine($"Then and Else should have the same type, but their types are '{then.ResultType}' and '{@else.ResultType}'");

            if (@if is ImmutableProcess<bool> ifProcess)
            {
                if(!string.IsNullOrWhiteSpace(errors.ToString()))
                    return Result.Failure<ImmutableProcess>(errors.ToString());

                ImmutableProcess ip;

                try
                {
                    ip = CreateImmutableConditional(ifProcess, then as dynamic, @else as dynamic, processSettings);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
                {
                    errors.AppendLine(e.Message);
                    return Result.Failure<ImmutableProcess>(errors.ToString());
                }
#pragma warning restore CA1031 // Do not catch general exception types

                return ip;
            }
            else
            {
                errors.AppendLine($"If process should have type bool");
                return Result.Failure<ImmutableProcess>(errors.ToString());
            }
        }

        private static ImmutableProcess CreateImmutableConditional<T>(ImmutableProcess<bool> ifP, ImmutableProcess<T> thenP, ImmutableProcess<T> elseP, IProcessSettings processSettings)
        {
            var conditional =  new Conditional<T>(ifP, thenP, elseP);

            if (ifP.ProcessConverter == null) return conditional;

            //Try converting this process
            var (isSuccess, _, value) = ifP.ProcessConverter.TryConvert(conditional, processSettings);

            if (isSuccess)
                return value;

            return conditional;
        }

    }
}