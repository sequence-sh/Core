using Reductech.EDR.Core.Internal.Parser;

namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// Extension methods for ErrorBuilders that involve adding a location.
    /// </summary>
    public static class ErrorLocationHelper
    {
        /// <summary>
        /// Add a StepErrorLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder, IStep step)=> errorBuilder.WithLocation(new StepErrorLocation(step));

        /// <summary>
        /// Add a FreezableStepErrorLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder, IFreezableStep step)=> errorBuilder.WithLocation(new FreezableStepErrorLocation(step));

        /// <summary>
        /// Add a FreezableStepErrorLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder,FreezableStepData data)=> errorBuilder.WithLocation(data.Location);

        /// <summary>
        /// Add a TextLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder, TextLocation token) => errorBuilder.WithLocation(token);
    }
}