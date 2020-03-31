using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class AssertBool : ImmutableProcess<Unit>
    {
        private readonly bool _expectedResult;

        private readonly ImmutableProcess<bool> _subProcess;

        public AssertBool(ImmutableProcess<bool> subProcess, bool expectedResult)
        {
            _subProcess = subProcess;
            _expectedResult = expectedResult;
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetAssertBoolProcessName(_subProcess.Name, _expectedResult);

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            bool? successState = null;
            bool? value = null;

            await foreach (var processOutput in _subProcess.Execute())
            {
                if (processOutput.OutputType == OutputType.Success && successState != false)
                {
                    successState = processOutput.Value == _expectedResult;
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
                        $"Assertion failed, value was {value.Value} but expected {_expectedResult}");
                    break;
                }
                default:
                    yield return ProcessOutput<Unit>.Error("$Assertion failed - inconclusive");
                    break;
            }
        }
    }
}