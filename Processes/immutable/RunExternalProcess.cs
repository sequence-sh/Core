using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class RunExternalProcess : ImmutableProcess<Unit>
    {
        /// <inheritdoc />
        public RunExternalProcess(string name, string processPath, IReadOnlyCollection<string> arguments) : base(name)
        {
            _processPath = processPath;
            _arguments = arguments;
        }

        private readonly string _processPath;

        private readonly IReadOnlyCollection<string> _arguments;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            var result = ExternalProcessHelper.RunExternalProcess(_processPath, _arguments);

            await foreach (var line in result)
                yield return line;
        }
    }
}