using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Repeat a step a set number of times.
    /// </summary>
    public sealed class RepeatXTimesStepFactory : SimpleStepFactory<RepeatXTimes, Unit>
    {
        private RepeatXTimesStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<RepeatXTimes, Unit> Instance { get; } = new RepeatXTimesStepFactory();


        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Repeat '[{nameof(RepeatXTimes.Action)}]' '[{nameof(RepeatXTimes.Number)}]' times.");
    }
}