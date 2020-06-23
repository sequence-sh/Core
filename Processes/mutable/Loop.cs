using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.chain;
using Reductech.EDR.Utilities.Processes.mutable.enumerations;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Performs a nested process once for each element in an enumeration.
    /// </summary>
    public class Loop : Process
    {
        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var freezeResult = TryFreeze(processSettings);

            return TryConvertFreezeResult<TOutput, Unit>(freezeResult);
        }

        private Result<IImmutableProcess<Unit>> TryFreeze(IProcessSettings processSettings)
        {
            var initialErrors = new StringBuilder();

            if (Do == null) initialErrors.AppendLine($"{nameof(Do)} is null");

            if(For == null) initialErrors.AppendLine($"{nameof(For)} is null");

            if (!string.IsNullOrWhiteSpace(initialErrors.ToString())  || For == null || Do == null)
                return Result.Failure<IImmutableProcess<Unit>>(initialErrors.ToString());

            var (_, isEnumerationFailure, elements, enumerationError) = For.TryGetElements(processSettings);

            if (isEnumerationFailure) return Result.Failure<IImmutableProcess<Unit>>(enumerationError);

            return elements switch
            {
                IEagerEnumerationElements eagerEnumerationElements =>
                GetFreezeResultFromEagerElements(processSettings, eagerEnumerationElements, Do),
                ILazyEnumerationElements lazyEnumerationElements =>
                Result.Success<IImmutableProcess<Unit>>(new LazyLoop(lazyEnumerationElements, Do, processSettings)),
                _ => Result.Failure<IImmutableProcess<Unit>>("Could not handle enumeration elements")
            };
        }

        internal static Result<IImmutableProcess<Unit>> GetFreezeResultFromEagerElements(IProcessSettings processSettings, IEagerEnumerationElements eagerEnumerationElements, Process @do)
        {
            var finalProcesses = new List<IImmutableProcess<Unit>>();

            foreach (var processInjector in eagerEnumerationElements.Injectors)
            {
                var subProcess = @do;

                var injectionResult = processInjector.Inject(subProcess);

                if (injectionResult.IsFailure)
                    return injectionResult.ConvertFailure<IImmutableProcess<Unit>>();

                var freezeResult = subProcess.TryFreeze<Unit>(processSettings);

                if (freezeResult.IsFailure) return freezeResult;

                finalProcesses.Add(freezeResult.Value);
            }

            var finalSequence = immutable.Sequence.CombineSteps(finalProcesses, processSettings);

            return Result.Success(finalSequence);
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return For.GetEnumerationStyle() switch
            {
                EnumerationStyle.Lazy => new ChainLinkBuilder<TInput, Unit, TFinal, LazyLoop, Loop>(this),
                EnumerationStyle.Eager => new ChainLinkBuilder<TInput, Unit, TFinal, immutable.Sequence, Loop>(this),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetLoopName(For.Name, Do.GetName());

        /// <summary>
        /// The enumeration to iterate through.
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Enumeration For { get; set; }

        /// <summary>
        /// The process to run once for each element.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        public Process Do { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override string ToString()
        {
            return GetName();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            if (Do == null)
                return Enumerable.Empty<string>();

            return Do.GetRequirements();
        }
    }
}
