using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class RunExternalProcess : ImmutableProcess
    {
        /// <inheritdoc />
        public RunExternalProcess(string name, string processPath, IReadOnlyDictionary<string, string> parameters) : base(name)
        {
            _processPath = processPath;
            _parameters = parameters;
        }

        private readonly string _processPath;

        private readonly IReadOnlyDictionary<string,string> _parameters;

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            var args = new List<string>();

            foreach (var (key, value) in _parameters)
            {
                args.Add($"-{key}");
                args.Add(value);
            }

            var result = ExternalProcessHelper.RunExternalProcess(_processPath, args);

            await foreach (var line in result)
            {
                yield return line;
            }
        }
    }
}