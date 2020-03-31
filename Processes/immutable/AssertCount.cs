using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class AssertCount : ImmutableProcess<Unit>
    {
        private readonly int? _minimum;
        private readonly int? _maximum;
        private readonly ImmutableProcess<int> _countProcess;

        public AssertCount(int? minimum, int? maximum, ImmutableProcess<int> countProcess)
        {
            _minimum = minimum;
            _maximum = maximum;
            _countProcess = countProcess;
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetAssertCountProcessName(_countProcess.Name);

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            bool? successState = null;
            int? value = null;

            await foreach (var countOutput in _countProcess.Execute())
            {
                if (countOutput.OutputType == OutputType.Success && successState != false)
                {
                    successState =
                        (!_minimum.HasValue || _minimum <= countOutput.Value) &&
                        (!_maximum.HasValue || _maximum >= countOutput.Value);
                    value = countOutput.Value;
                }
                else
                {
                    if (countOutput.OutputType == OutputType.Error) successState = false;
                    yield return countOutput.ConvertTo<Unit>();
                }
            }

            switch (successState)
            {
                case true:
                    yield return ProcessOutput<Unit>.Success(Unit.Instance);
                    break;
                case false when value.HasValue:
                {
                    string expected;
                    if (_minimum.HasValue && _maximum.HasValue)
                        expected = $"between {_minimum.Value} and {_maximum.Value}";
                    else if (_minimum.HasValue)
                        expected = $"greater than {_minimum}";
                    else if (_maximum.HasValue)
                        expected = $"less than {_maximum}";
                    else expected = "unknown";

                    yield return ProcessOutput<Unit>.Error(
                        $"Assertion failed, value was {value.Value} but expected {expected}");
                    break;
                }
                default:
                    yield return ProcessOutput<Unit>.Error("$Assertion failed - inconclusive");
                    break;
            }

        }
    }
}
