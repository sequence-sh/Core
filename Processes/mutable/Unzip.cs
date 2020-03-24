using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Unzips a file.
    /// </summary>
    public class Unzip : Process
    {
        /// <summary>
        /// The path to the archive to unzip.
        /// </summary>
        [Required]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ArchiveFilePath { get; set; }


        /// <summary>
        /// The path to the directory in which to place the extracted files. 
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
        public string DestinationDirectory { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// Should files be overwritten in the destination directory.
        /// </summary>
        [YamlMember(Order = 3)]
        public bool OverwriteFiles { get; } = false;

        /// <inheritdoc />
        public override string GetName() => $"Unzip {nameof(ArchiveFilePath)}";

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ArchiveFilePath)) 
                errors.Add($"{nameof(ArchiveFilePath)} is empty.");

            if (string.IsNullOrWhiteSpace(DestinationDirectory)) 
                errors.Add($"{nameof(DestinationDirectory)} is empty.");

            if (errors.Any())
                return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(errors));

            var ip = new immutable.Unzip(GetName(), ArchiveFilePath, DestinationDirectory, OverwriteFiles);

            return Result.Success<ImmutableProcess, ErrorList>(ip);

        }
    }
}