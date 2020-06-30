using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// Executes each step, one after the another.
    /// Will stop if a process fails.
    /// </summary>
    public class Sequence : Process
    {
        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName()
        {
            return ProcessNameHelper.GetSequenceName(Steps.Select(s => s.GetName()));
        }

        /// <summary>
        /// Steps that make up this sequence.
        /// These should all have result type void.
        /// </summary>
        [Required]

        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public List<Process> Steps { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            return TryConvertFreezeResult<TOutput, Unit>(TryFreeze(processSettings));
        }

        private Result<IImmutableProcess<Unit>> TryFreeze(IProcessSettings processSettings)
        {
            var r = Steps.Select(s => s.TryFreeze<Unit>(processSettings))
                .Combine("\r\n");

            if (r.IsFailure)
                return r.ConvertFailure<IImmutableProcess<Unit>>();

            var steps = r.Value;
            var p = Immutable.Sequence.CombineSteps(steps, processSettings);

            return Result.Success(p);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            if (Steps == null)
                return Enumerable.Empty<string>();

            return Steps.SelectMany(x => x.GetRequirements()).Distinct();
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput,Unit,TFinal,Immutable.Sequence,Sequence>(this);
        }
    }
}