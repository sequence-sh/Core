using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable
{
    internal class DeleteItem : ImmutableProcess<Unit>
    {
        /// <inheritdoc />
        public DeleteItem(string path)
        {
            _path = path;
        }

        private readonly string _path;

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (Directory.Exists(_path))
            {
                Directory.Delete(_path, true);
                yield return ProcessOutput<Unit>.Message("Directory Deleted");
            }
            else if (File.Exists(_path))
            {
                File.Delete(_path);
                yield return ProcessOutput<Unit>.Message("File Deleted");
            }
            else
            {
                yield return ProcessOutput<Unit>.Message("Item did not exist.");
            }
            yield return ProcessOutput<Unit>.Success(Unit.Instance);
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetDeleteItemName();

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}