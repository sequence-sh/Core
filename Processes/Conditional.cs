﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Linq;
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
        public override IEnumerable<string> GetArgumentErrors()
        {
            return If.GetArgumentErrors().Concat(Then.GetArgumentErrors()).Concat(Else?.GetArgumentErrors()??Enumerable.Empty<string>());
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            return If.GetSettingsErrors(processSettings).Concat(Then.GetSettingsErrors(processSettings)).Concat(Else?.GetSettingsErrors(processSettings)??Enumerable.Empty<string>());
        }
        /// <inheritdoc />
        public override string GetName() => Else == null? $"If ({If}) then ({Then})" : $"If ({If}) then ({Then}) else ({Else})";
            
        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
        {
            yield return Result.Success($"Testing {If}");

            var success = true;
            await foreach (var r in If.Execute(processSettings))
            {
                if (r.IsSuccess)
                    yield return r;
                else
                {
                    success = false;
                    yield return Result.Success(r.Error); //These methods failing is expected so it should not produce an error
                }
            }

            if (success)
            {
                yield return Result.Success($"Assertion Succeeded, executing {Then}");

                await foreach (var r in Then.Execute(processSettings))
                    yield return r;
            }
            else if (Else != null)
            {
                yield return Result.Success($"Assertion Failed, executing {Else}");

                await foreach (var r in Else.Execute(processSettings))
                    yield return r;
            }
            else
                yield return Result.Success("Assertion Failed");
        }
    }
}