using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class Unzip : ImmutableProcess<Unit>
    {
        /// <inheritdoc />
        public Unzip(string archiveFilePath, string destinationDirectory, bool overwriteFiles)
        {
            _archiveFilePath = archiveFilePath;
            _destinationDirectory = destinationDirectory;
            _overwriteFiles = overwriteFiles;
        }

        private readonly string _archiveFilePath;

        private readonly string _destinationDirectory;

        private readonly bool _overwriteFiles;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            yield return ProcessOutput<Unit>.Message($"Unzipping + '{nameof(_archiveFilePath)}' to '{nameof(_destinationDirectory)}'");

            var error = await Task.Run(() => Extract(_archiveFilePath, _destinationDirectory, _overwriteFiles));

            if(error != null)
                yield return ProcessOutput<Unit>.Error(error);

            yield return ProcessOutput<Unit>.Success(Unit.Instance);
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

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetUnzipName();
    }
}