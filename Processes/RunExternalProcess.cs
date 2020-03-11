using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Runs an external process.
    /// </summary>
    public class RunExternalProcess : Process
    {
        /// <summary>
        /// The path to the process to run
        /// </summary>
        [YamlMember(Order = 2)]
        [Required]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ProcessPath { get; set; }

        /// <summary>
        /// Pairs of parameters to give to the process
        /// </summary>
        [YamlMember(Order = 3)]
        [Required]
        public Dictionary<string, string> Parameters { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            if (ProcessPath == null)
                yield return $"{nameof(ProcessPath)} must not be null.";
            else if (!ProcessPath.EndsWith(".exe"))
                yield return $"'{ProcessPath}' does not point to an executable file.";
            if (!File.Exists(ProcessPath))
                yield return $"'{ProcessPath}' does not exist.";
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            yield break;
        }

        /// <inheritdoc />
        public override string GetName()
        {
            return $"{ProcessPath} {string.Join(" ", Parameters.Select((k, v) => $"-{k} {v}"))}";
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
        {
            var argumentErrors = GetArgumentErrors().Concat(GetSettingsErrors(processSettings)).ToList();

            if (argumentErrors.Any())
            {
                foreach (var ae in argumentErrors)
                    yield return Result.Failure<string>(ae);
                yield break;
            }

            var args = new List<string>();

            foreach (var (key, value) in Parameters)
            {
                args.Add($"-{key}");
                args.Add(value);
            }

            var result = ExternalProcessHelper.RunExternalProcess(ProcessPath, args);

            await foreach (var line in result)
            {
                    yield return line;
            }
        }
    }
}
