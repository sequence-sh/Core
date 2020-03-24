using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// A process that does nothing;
    /// </summary>
    internal class DoNothing : ImmutableProcess
    {
        public static readonly ImmutableProcess Instance = new DoNothing();

        /// <inheritdoc />
        private DoNothing() : base("Do Nothing")
        {
        }

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async IAsyncEnumerable<Result<string>> Execute()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            yield break;
        }
    }
}