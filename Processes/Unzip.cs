using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes
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
        public override IEnumerable<string> GetArgumentErrors()
        {
            if (string.IsNullOrWhiteSpace(ArchiveFilePath))
                yield return $"{nameof(ArchiveFilePath)} is empty.";

            if (string.IsNullOrWhiteSpace(DestinationDirectory))
                yield return $"{nameof(DestinationDirectory)} is empty.";
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            yield break;
        }

        /// <inheritdoc />
        public override string GetName() => $"Unzip {nameof(ArchiveFilePath)}";

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
        {
            yield return Result.Success($"Unzipping + '{nameof(ArchiveFilePath)}' to '{nameof(DestinationDirectory)}'");

            var error = await Task.Run(() => Extract(ArchiveFilePath, DestinationDirectory, OverwriteFiles));

            if(error != null)
                yield return Result.Failure<string>(error);

            yield return Result.Success("File Unzipped");
        }

        private static string? Extract(string archiveFilePath, string destinationDirectory, bool overwriteFile)
        {
            string? error;
            try
            {
                ZipFile.ExtractToDirectory(archiveFilePath, destinationDirectory, overwriteFile);
                error = null;
            }
            catch (Exception e)
            {
                error = e.Message ?? "Unknown Error";
            }

            return error;
        }
    }
}