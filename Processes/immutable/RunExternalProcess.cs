using System.Collections.Generic;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable
{
    internal class RunExternalProcess : ImmutableProcess<Unit>
    {
        /// <inheritdoc />
        public RunExternalProcess( string processPath, IReadOnlyCollection<string> arguments)
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

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetRunExternalProcessName();

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}