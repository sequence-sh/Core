using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Easy access to common errors.
    /// </summary>
    public static class ErrorHelper
    {
        /// <summary>
        /// The error that should be returned when a parameter is missing.
        /// </summary>
        public static IErrorBuilder MissingParameterError(string parameterName) => new ErrorBuilder($"Missing Parameter '{parameterName}'", ErrorCode.MissingParameter);
    }
}