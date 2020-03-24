using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Executes each step, one after the another.
    /// Will stop if a process fails.
    /// </summary>
    public class Sequence : Process
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName()
        {
            return string.Join(" then ", Steps.Select(s=>s.GetName()));
        }

        /// <summary>
        /// Steps that make up this process.
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

            var immutableSequence = ImmutableSequence.CombineSteps(r.Value);

            return Result.Success<ImmutableProcess, ErrorList>(immutableSequence);

        }
    }
}