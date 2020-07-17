using System.Collections.Generic;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable
{
    /// <summary>
    /// Outputs a particular message.
    /// </summary>
    public class OutputMessage : ImmutableProcess<Unit>
    {
        /// <inheritdoc />
        public OutputMessage(string message)
        {
            _message = message;
        }

        private readonly string _message;

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            yield return ProcessOutput<Unit>.Message(_message);
            yield return ProcessOutput<Unit>.Success(Unit.Instance);
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetOutputMessageProcessName(_message);

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}