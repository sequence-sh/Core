using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Runs an external process.
    /// </summary>
    public class RunExternalProcess : Process
    {
        /// <summary>
        /// The path to the process to run.
        /// </summary>
        [YamlMember(Order = 2)]
        [Required]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ProcessPath { get; set; }

        /// <summary>
        /// Pairs of parameters to give to the process.
        /// </summary>
        [YamlMember(Order = 3)]
        [Required]
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override string GetName()
        {
            return $"{ProcessPath} {string.Join(" ", Parameters.Select((k, v) => $"-{k} {v}"))}";
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ProcessPath))
                errors.Add($"{nameof(ProcessPath)} must not be empty.");
            else if (!ProcessPath.EndsWith(".exe"))
                errors.Add($"'{ProcessPath}' does not point to an executable file.");
            if (!File.Exists(ProcessPath))
                errors.Add($"'{ProcessPath}' does not exist.");

            if (errors.Any())
                return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(errors));
            
            var ip = new immutable.RunExternalProcess(GetName(), ProcessPath, Parameters);

            return Result.Success<ImmutableProcess, ErrorList>(ip);
        }
    }
}
