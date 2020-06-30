using System.Collections.Generic;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable
{
    /// <summary>
    /// Asserts that a particular process will produce an error.
    /// </summary>
    public class AssertError : ImmutableProcess<Unit>
    {
        /// <summary>
        /// The process that is expected to produce an error.
        /// </summary>
        public readonly IImmutableProcess<Unit> SubProcess;

        /// <inheritdoc />
        public AssertError(IImmutableProcess<Unit> subProcess)
        {
            SubProcess = subProcess;
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            var failed = false;

            var results = SubProcess.Execute();
            await foreach (var line in results)
            {
                if (line.OutputType == OutputType.Message || line.OutputType == OutputType.Warning)
                    yield return line.ConvertTo<Unit>();
                else if(line.OutputType == OutputType.Error) //we don't need to see the error message.
                    failed = true;
                else if (line.OutputType == OutputType.Success) //we don't need to see the error message.
                    failed = false;
            }

            if (failed)
                yield return ProcessOutput<Unit>.Success(Unit.Instance);
            else
            {
                yield return ProcessOutput<Unit>.Error("Assertion Failed - Process was unexpectedly successful");
            }
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetAssertErrorName(SubProcess.Name);

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}