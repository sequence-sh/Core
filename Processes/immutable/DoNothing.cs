using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// A process that does nothing;
    /// </summary>
    internal class DoNothing : ImmutableProcess<Unit>
    {
        public static readonly ImmutableProcess Instance = new DoNothing();

        /// <inheritdoc />
        private DoNothing()
        {
        }

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            yield return ProcessOutput<Unit>.Success(Unit.Instance);
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetDoNothingName();

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}