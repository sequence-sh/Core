using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// SerializationMethods methods for processes.
    /// </summary>
    public static class FreezableProcessExtensions
    {
        /// <summary>
        /// Tries to freeze this step.
        /// </summary>
        public static Result<IStep> TryFreeze(this IFreezableStep step) =>
            StepContext.TryCreate(step)
                .Bind(step.TryFreeze);


    }
}