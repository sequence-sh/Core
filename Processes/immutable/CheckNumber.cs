using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class CheckNumber : ImmutableProcess<bool>
    {
        private readonly int? _minimum;
        private readonly int? _maximum;
        private readonly ImmutableProcess<int> _countProcess;

        public CheckNumber(int? minimum, int? maximum, ImmutableProcess<int> countProcess)
        {
            _minimum = minimum;
            _maximum = maximum;
            _countProcess = countProcess;
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetCheckNumberProcessName(_countProcess.Name);

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<bool>> Execute()
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
                    yield return countOutput.ConvertTo<bool>();
                }
            }

            switch (successState)
            {
                case true:
                    yield return ProcessOutput<bool>.Success(true);
                    break;
                case false when value.HasValue:
                    yield return ProcessOutput<bool>.Success(false);
                    break;
                default:
                    yield return ProcessOutput<bool>.Error("Check failed - inconclusive");
                    break;
            }

        }
    }
}
