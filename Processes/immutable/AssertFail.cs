using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// Asserts that a particular process will fail.
    /// </summary>
    internal class AssertFail : ImmutableProcess
    {
        private readonly ImmutableProcess _subProcess;

        /// <inheritdoc />
        public AssertFail(string name, ImmutableProcess subProcess) : base(name)
        {
            _subProcess = subProcess;
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            var failed = false;

            var results = _subProcess.Execute();
            await foreach (var line in results)
            {
                if (line.IsSuccess)
                    yield return line;
                else
                {
                    yield return Result.Success(line.Error);
                    failed = true;
                }
            }

            if (failed)
                yield return Result.Success("Assertion Succeeded");
            else
            {
                yield return Result.Failure<string>("Assertion Failed - Process was unexpectedly successful");
            }
        }
    }
}