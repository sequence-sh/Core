using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using Reductech.EDR.Utilities.Processes.output;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
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
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var r = Steps.Select(s => s.TryFreeze(processSettings)).Combine(ErrorList.Compose);

            if (r.IsFailure)
                return r.ConvertFailure<ImmutableProcess>();

            var steps = r.Value;
            var unitSteps = new List<ImmutableProcess<Unit>>();

            foreach (var s in steps)
                if (s is ImmutableProcess<Unit> ipu)
                    unitSteps.Add(ipu);
                else
                    return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(
                        $"Process '{s.Name}' has result type {s.ResultType.Name} but members of a sequence should have result type void."));
            var immutableProcess = immutable.Sequence.CombineSteps(unitSteps, processSettings);

            return Result.Success<ImmutableProcess, ErrorList>(immutableProcess);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            if (Steps == null)
                return Enumerable.Empty<string>();

            return Steps.SelectMany(x => x.GetRequirements()).Distinct();
        }
    }
}