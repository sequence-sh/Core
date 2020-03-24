using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class Unzip : ImmutableProcess
    {
        /// <inheritdoc />
        public Unzip(string name, string archiveFilePath, string destinationDirectory, bool overwriteFiles) : base(name)
        {
            _archiveFilePath = archiveFilePath;
            _destinationDirectory = destinationDirectory;
            _overwriteFiles = overwriteFiles;
        }

        private readonly string _archiveFilePath;

        private readonly string _destinationDirectory;

        private readonly bool _overwriteFiles;

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            yield return Result.Success($"Unzipping + '{nameof(_archiveFilePath)}' to '{nameof(_destinationDirectory)}'");

            var error = await Task.Run(() => Extract(_archiveFilePath, _destinationDirectory, _overwriteFiles));

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