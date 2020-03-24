using System.ComponentModel.DataAnnotations;
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
        /// </summary>
        [Required]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Process If { get; set; }

        /// <summary>
        /// If the 'If' process was successful then run this.
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
        public Process Then { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// If the 'If' process was unsuccessful then run this.
        /// </summary>
        
        [YamlMember(Order = 3)]
        public Process? Else { get; set; }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var ifResult = If.TryFreeze(processSettings);
            var thenResult = Then.TryFreeze(processSettings);


            var elseResult = Else?.TryFreeze(processSettings) ?? Result.Success<ImmutableProcess, ErrorList>(DoNothing.Instance);

            var combinedResult = Result.Combine(ErrorList.Compose, ifResult, thenResult, elseResult);

            if (combinedResult.IsFailure) return combinedResult.ConvertFailure<ImmutableProcess>();
            var newProcess = new immutable.Conditional(GetName(), ifResult.Value, thenResult.Value, elseResult.Value);

            return Result.Success<ImmutableProcess, ErrorList>(newProcess);
        }

        /// <inheritdoc />
        public override string GetName() => Else == null? $"If ({If}) then ({Then})" : $"If ({If}) then ({Then}) else ({Else})";
    }
}