using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class DoesFileContain : ImmutableProcess<bool>
    {
        /// <inheritdoc />
        public DoesFileContain(string filePath, string expectedContents) 
        {
            _filePath = filePath;
            _expectedContents = expectedContents;
        }

        private readonly string _filePath;

        private readonly string _expectedContents;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<bool>> Execute()
        {
            if (!File.Exists(_filePath))
                yield return ProcessOutput<bool>.Error("File does not exist");
            else
            {
                var text = await File.ReadAllTextAsync(_filePath);

                if (text.Contains(_expectedContents))
                    yield return ProcessOutput<bool>.Success(true);
                else
                {
                    yield return ProcessOutput<bool>.Success(false);
                }
            }
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetAssertFileContainsProcessName();

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}
