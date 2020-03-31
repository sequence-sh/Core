using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class ImmutableAssertFileContents : ImmutableProcess<Unit> //TODO move into processes
    {
        /// <inheritdoc />
        public ImmutableAssertFileContents(string filePath, string expectedContents) 
        {
            _filePath = filePath;
            _expectedContents = expectedContents;
        }

        private readonly string _filePath;

        private readonly string _expectedContents;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            if (!File.Exists(_filePath))
                yield return ProcessOutput<Unit>.Error("File does not exist");
            else
            {
                var text = await File.ReadAllTextAsync(_filePath);

                if (text.Contains(_expectedContents))
                    yield return ProcessOutput<Unit>.Success(Unit.Instance);
                else
                {
                    yield return ProcessOutput<Unit>.Error("Contents do not match");
                }
            }
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetAssertFileContainsProcessName();
    }
}
