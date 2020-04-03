using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// Assert that a particular process with produce true.
    /// </summary>
    public class AssertBool : ImmutableProcess<Unit>
    {
        /// <summary>
        /// The expected result of the sub process.
        /// </summary>
        public readonly bool ExpectedResult;


        /// <summary>
        /// The subprocess.
        /// </summary>
        public readonly ImmutableProcess<bool> SubProcess;

        /// <summary>
        /// Creates a new AssertBool process.
        /// </summary>
        /// <param name="subProcess"></param>
        /// <param name="expectedResult"></param>
        public AssertBool(ImmutableProcess<bool> subProcess, bool expectedResult)
        {
            SubProcess = subProcess;
            ExpectedResult = expectedResult;
        }

        

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetAssertBoolProcessName(SubProcess.Name, ExpectedResult);

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            bool? successState = null;
            bool? value = null;

            await foreach (var processOutput in SubProcess.Execute())
            {
                if (processOutput.OutputType == OutputType.Success && successState != false)
                {
                    successState = processOutput.Value == ExpectedResult;
                    value = processOutput.Value;
                }
                else
                {
                    if (processOutput.OutputType == OutputType.Error) successState = false;
                    yield return processOutput.ConvertTo<Unit>();
                }
            }

            switch (successState)
            {
                case true:
                    yield return ProcessOutput<Unit>.Success(Unit.Instance);
                    break;
                case false when value.HasValue:
                {
                    yield return ProcessOutput<Unit>.Error(
                        $"Assertion failed, value was {value.Value} but expected {ExpectedResult}");
                    break;
                }
                default:
                    yield return ProcessOutput<Unit>.Error("$Assertion failed - inconclusive");
                    break;
            }
        }
    }
}