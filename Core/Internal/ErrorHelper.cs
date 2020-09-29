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
        public static IRunErrors MissingParameterError(string parameterName, string stepName)
            => new RunError($"Missing Parameter '{parameterName}'", stepName, null , ErrorCode.MissingParameter);
    }
}