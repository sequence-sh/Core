using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
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
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var ifResult = If?.TryFreeze<bool>(processSettings)?? Result.Failure<IImmutableProcess<bool>>($"'{nameof(If)}' must be set.");
            var thenResult = Then?.TryFreeze<TOutput>(processSettings)?? Result.Failure<IImmutableProcess<TOutput>>($"'{nameof(Then)}' must be set.");
            var elseResult1 = Else?.TryFreeze<TOutput>(processSettings);// ?? Result.Success(DoNothing.Instance);

            Result<IImmutableProcess<TOutput>> elseResult;
            if (elseResult1 == null)
            {
                if (DoNothing.Instance is IImmutableProcess<TOutput> doNothing)
                    elseResult = Result.Success(doNothing);
                else
                    elseResult =
                        Result.Failure<IImmutableProcess<TOutput>>(
                            $"'{nameof(Else)}' must be set in typed conditionals.");
            }
            else elseResult = elseResult1.Value;

            var combinedError = new StringBuilder();
            if (ifResult.IsFailure) combinedError.AppendLine(ifResult.Error);
            if (thenResult.IsFailure) combinedError.AppendLine(thenResult.Error);
            if (elseResult.IsFailure) combinedError.AppendLine(elseResult.Error);


            if (!string.IsNullOrWhiteSpace(combinedError.ToString())) return Result.Failure<IImmutableProcess<TOutput>>(combinedError.ToString());

            var createResult = CreateImmutableConditional(ifResult.Value, thenResult.Value, elseResult.Value, processSettings);

            return Result.Success(createResult);
        }

        /// <inheritdoc />
        public override IEnumerable<Requirement> GetAllRequirements()
        {
            if (If == null || Then == null)
                return base.GetAllRequirements();

            return base.GetAllRequirements().Concat(If.GetAllRequirements().Concat(Then.GetAllRequirements())
                    .Concat(Else?.GetAllRequirements() ?? Enumerable.Empty<Requirement>())).Distinct();
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return Result.Failure<ChainLinkBuilder<TInput, TFinal>>("Cannot nest a chain within a chain"); //TODO find a way to do this. It should be possible
        }

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => "Returns the same type as the 'Then' and 'Else' processes. Returns void if there is no Else process.";

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetConditionalName(If.GetName(), Then.GetName(), Else?.GetName());

        private static IImmutableProcess<T> CreateImmutableConditional<T>(IImmutableProcess<bool> ifP, IImmutableProcess<T> thenP, IImmutableProcess<T> elseP, IProcessSettings processSettings)
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