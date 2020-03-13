using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Deletes a file or a directory.
    /// </summary>
    public class DeleteItem : Process
    {
        /// <summary>
        /// The path to the file or directory to delete.
        /// </summary>
        [YamlMember(Order = 2)]
        [Required]
        
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Path { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            if (string.IsNullOrWhiteSpace(Path))
                yield return "File Path is empty";
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            yield break;
        }

        /// <inheritdoc />
        public override string GetName() => $"Delete {Path}";

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously - needs to be async to return IAsyncEnumerable
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
                yield return Result.Success("Directory deleted");
            }
            else if (File.Exists(Path))
            {
                File.Delete(Path);
                yield return Result.Success("File deleted");
            }
            else
            {
                yield return Result.Success("File did not exist");
            }
        }
    }
}