using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable
{
    /// <summary>
    /// Creates a new directory in the file system.
    /// </summary>
    internal class CreateDirectory : ImmutableProcess<Unit>
    {
        /// <inheritdoc />
        public CreateDirectory(string path)
        {
            _path = path;
        }

        private readonly string _path;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            if (string.IsNullOrWhiteSpace(_path))
                yield return ProcessOutput<Unit>.Message("Path is empty");
            else
            {
                var r = await Task.Run(() => TryCreateDirectory(_path));
                yield return r;
            }
        }

        private static IProcessOutput<Unit> TryCreateDirectory(string path)
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

            if (error != null)
                return ProcessOutput<Unit>.Error(error);
            return ProcessOutput<Unit>.Success(Unit.Instance);
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetCreateDirectoryName();

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}