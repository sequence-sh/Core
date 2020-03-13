using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Creates a new directory
    /// </summary>
    public class CreateDirectory : Process
    {
        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            if (string.IsNullOrWhiteSpace(Path))
                yield return "Path must not be empty";
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            yield break;
        }

        /// <inheritdoc />
        public override string GetName() => $"Create Directory {Path}";

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
        {
            if (string.IsNullOrWhiteSpace(Path))
                yield return Result.Failure<string>("Path is empty");
            else
            {
                var r = await Task.Run(() => TryCreateDirectory(Path));
                yield return r;
            }
        }

        private static Result<string> TryCreateDirectory(string path)
        {
            string? error;
            try
            {
                Directory.CreateDirectory(path);
                error = null;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                error = e.Message;
            }
#pragma warning restore CA1031 // Do not catch general exception types
            return error != null ? Result.Failure<string>(error) : Result.Success("Directory Created");
        }

        /// <summary>
        /// The path to the directory to create.
        /// </summary>
        [YamlMember(Order = 2)]
        [Required]
        [DataMember]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Path { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}