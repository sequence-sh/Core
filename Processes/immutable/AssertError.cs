using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// Asserts that a particular process will produce an error.
    /// </summary>
    internal class AssertError : ImmutableProcess<Unit>
    {
        private readonly ImmutableProcess _subProcess;

        /// <inheritdoc />
        public AssertError(ImmutableProcess subProcess)
        {
            _subProcess = subProcess;
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            var failed = false;

            var results = _subProcess.ExecuteUntyped();
            await foreach (var line in results)
            {
                if (line.OutputType != OutputType.Success)
                    yield return line.ConvertTo<Unit>();
                else //we don't need to see the error message.
                    failed = true;
            }

            if (failed)
                yield return ProcessOutput<Unit>.Success(Unit.Instance);
            else
            {
                yield return ProcessOutput<Unit>.Error("Assertion Failed - Process was unexpectedly successful");
            }
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetAssertErrorName(_subProcess.Name);

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}