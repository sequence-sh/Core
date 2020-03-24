using System.Collections.Generic;
using System.IO;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class DeleteItem : ImmutableProcess
    {
        /// <inheritdoc />
        public DeleteItem(string name, string path) : base(name)
        {
            _path = path;
        }

        private readonly string _path;

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async IAsyncEnumerable<Result<string>> Execute()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (Directory.Exists(_path))
            {
                Directory.Delete(_path, true);
                yield return Result.Success("Directory deleted");
            }
            else if (File.Exists(_path))
            {
                File.Delete(_path);
                yield return Result.Success("File deleted");
            }
            else
            {
                yield return Result.Success("File did not exist");
            }
        }
    }
}