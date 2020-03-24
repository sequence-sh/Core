using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// Creates a new directory in the file system.
    /// </summary>
    internal class CreateDirectory : ImmutableProcess
    {
        /// <inheritdoc />
        public CreateDirectory(string name, string path) : base(name)
        {
            _path = path;
        }

        private readonly string _path;

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            if (string.IsNullOrWhiteSpace(_path))
                yield return Result.Failure<string>("Path is empty");
            else
            {
                var r = await Task.Run(() => TryCreateDirectory(_path));
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
    }
}