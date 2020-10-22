using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

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
        public static Result<IStep, IError> TryFreeze(this IFreezableStep step) =>
            StepContext.TryCreate(step)
                .Bind(step.TryFreeze);


    }
}