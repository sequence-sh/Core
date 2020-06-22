using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// Returns whether a particular process produced a number within a particular range.
    /// </summary>
    public class CheckNumber : ImmutableProcess<bool>
    {
        /// <summary>
        /// Inclusive minimum of the expected range.
        /// </summary>
        public readonly int? Minimum;
        /// <summary>
        /// Inclusive maximum of the expected range.
        /// </summary>
        public readonly int? Maximum;
        /// <summary>
        /// The process that returns the number.
        /// </summary>
        public readonly IImmutableProcess<int> CountProcess;

        /// <summary>
        /// Create a new CheckNumber process
        /// </summary>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <param name="countProcess"></param>
        public CheckNumber(int? minimum, int? maximum, IImmutableProcess<int> countProcess)
        {
            Minimum = minimum;
            Maximum = maximum;
            CountProcess = countProcess;
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetCheckNumberProcessName(CountProcess.Name);

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<bool>> Execute()
        {
            bool? successState = null;
            int? value = null;

            await foreach (var countOutput in CountProcess.Execute())
            {
                if (countOutput.OutputType == OutputType.Success && successState != false)
                {
                    successState =
                        (!Minimum.HasValue || Minimum <= countOutput.Value) &&
                        (!Maximum.HasValue || Maximum >= countOutput.Value);
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
