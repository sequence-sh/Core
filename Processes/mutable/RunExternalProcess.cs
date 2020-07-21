using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
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
        /// Arguments to give to the process.
        /// </summary>
        [YamlMember(Order = 3)]
        [Required]
        public List<string> Arguments { get; set; } = new List<string>();
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetRunExternalProcessName();

        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            return TryConvertFreezeResult<TOutput, Unit>(TryFreeze());
        }

        private Result<IImmutableProcess<Unit>> TryFreeze()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ProcessPath))
                errors.Add($"{nameof(ProcessPath)} must not be empty.");
            else if (!ProcessPath.EndsWith(".exe"))
                errors.Add($"'{ProcessPath}' does not point to an executable file.");
            if (!File.Exists(ProcessPath))
                errors.Add($"'{ProcessPath}' does not exist.");

            if (errors.Any())
                return Result.Failure<IImmutableProcess<Unit>>(string.Join("\r\n", errors));

            var ip = new Immutable.RunExternalProcess(ProcessPath, Arguments);

            return Result.Success<IImmutableProcess<Unit>>(ip);
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput,Unit,TFinal,Immutable.RunExternalProcess,RunExternalProcess>(this);
        }
    }
}
