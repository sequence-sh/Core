namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// The step where the error originated
    /// </summary>
    public class StepErrorLocation : IErrorLocation
    {
        /// <summary>
        /// Creates a new StepErrorLocation
        /// </summary>
        /// <param name="step"></param>
        public StepErrorLocation(IStep step) => Step = step;

        /// <summary>
        /// The step
        /// </summary>
        public IStep Step { get; }

        /// <inheritdoc />
        public string AsString => Step.Name;
    }
}