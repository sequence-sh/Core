namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// The freezable step where the error originated
    /// </summary>
    public class FreezableStepErrorLocation : IErrorLocation
    {
        /// <summary>
        /// Creates a new FreezableStepErrorLocation
        /// </summary>
        public FreezableStepErrorLocation(IFreezableStep freezableStep) => FreezableStep = freezableStep;

        /// <summary>
        /// Creates a new FreezableStepErrorLocation
        /// </summary>
        public FreezableStepErrorLocation(IStepFactory stepFactory, FreezableStepData data) => FreezableStep = new CompoundFreezableStep(stepFactory, data, null);

        /// <summary>
        /// The freezable step
        /// </summary>
        public IFreezableStep FreezableStep { get; }

        /// <inheritdoc />
        public string AsString => FreezableStep.StepName;
    }
}