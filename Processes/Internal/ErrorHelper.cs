namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// Easy access to common errors.
    /// </summary>
    public static class ErrorHelper
    {
        /// <summary>
        /// The error that should be returned when a parameter is missing.
        /// </summary>
        public static IRunErrors MissingParameterError(string parameterName, string processName)
            => new RunError($"Missing Parameter '{parameterName}'", processName, null , ErrorCode.MissingParameter);
    }
}