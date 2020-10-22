namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Identifying code for an error message.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// Variable does not exist.
        /// </summary>
        MissingVariable,

        /// <summary>
        /// Variable has the wrong type.
        /// </summary>
        WrongVariableType,

        /// <summary>
        /// Index was out of the range of an array or string.
        /// </summary>
        IndexOutOfBounds,

        /// <summary>
        /// An error in an external step.
        /// </summary>
        ExternalProcessError,

        /// <summary>
        /// The external step did not return an output of the expected form.
        /// </summary>
        ExternalProcessMissingOutput,

        /// <summary>
        /// The external step was not found.
        /// </summary>
        ExternalProcessNotFound,

        /// <summary>
        /// The requirements for a step were not met.
        /// </summary>
        RequirementsNotMet,

        /// <summary>
        /// Cast failed.
        /// </summary>
        InvalidCast,

        /// <summary>
        /// Step settings are missing
        /// </summary>
        MissingStepSettings,

        /// <summary>
        /// Attempt to divide by zero
        /// </summary>
        DivideByZero,

        /// <summary>
        /// A required parameter was not set.
        /// </summary>
        MissingParameter,

        /// <summary>
        /// Parameters conflict.
        /// </summary>
        ConflictingParameters,

        /// <summary>
        /// An assertion failed
        /// </summary>
        AssertionFailed,

        /// <summary>
        /// An error reading a CSV file
        /// </summary>
        CSVError,

        /// <summary>
        /// Property did not exist
        /// </summary>
        InvalidProperty,

        /// <summary>
        /// The variable was defined with multiple conflicting types.
        /// </summary>
        AmbiguousType,

        /// <summary>
        /// The type of the variable could not be resolved.
        /// </summary>
        CouldNotResolveVariable,

        /// <summary>
        /// The sequence was empty.
        /// </summary>
        EmptySequence,

        /// <summary>
        /// The step could not be deserialized.
        /// </summary>
        CouldNotDeserialize
    }
}