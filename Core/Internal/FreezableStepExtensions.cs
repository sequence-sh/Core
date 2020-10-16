using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// SerializationMethods methods for steps.
    /// </summary>
    public static class FreezableStepExtensions
    {
        /// <summary>
        /// Tries to freeze this step.
        /// </summary>
        public static Result<IStep> TryFreeze(this IFreezableStep step) =>
            StepContext.TryCreate(step)
                .Bind(step.TryFreeze);


    }
}