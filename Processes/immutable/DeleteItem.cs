using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class DeleteItem : ImmutableProcess<Unit>
    {
        /// <inheritdoc />
        public DeleteItem(string name, string path) : base(name)
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
    }
}