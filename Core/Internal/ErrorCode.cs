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
    /// The external process did not return an output of the expected form.
    /// </summary>
    ExternalProcessMissingOutput,

    /// <summary>
    /// The external step was not found.
    /// </summary>
    ExternalProcessNotFound,

    /// <summary>
    /// The requirements for a step were not met.
    /// </summary>
    RequirementNotMet,

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
    /// The type of the variable could not be resolved.
    /// </summary>
    CouldNotResolveVariable,

    /// <summary>
    /// The sequence was empty.
    /// </summary>
    EmptySequence,

    /// <summary>
    /// The step with the given name does not exist.
    /// </summary>
    StepDoesNotExist,

    /// <summary>
    /// The same argument was given more than once
    /// </summary>
    DuplicateParameter,

    /// <summary>
    /// The term could not be tokenized
    /// </summary>
    CouldNotTokenize,

    /// <summary>
    /// The term could not be parsed
    /// </summary>
    CouldNotParse,

    /// <summary>
    /// A unexpected parameter was found
    /// </summary>
    UnexpectedParameter,

    /// <summary>
    /// The enum value could not be handled.
    /// </summary>
    UnexpectedEnumValue,

    UnexpectedEnumType,

    /// <summary>
    /// Intentional error to be used during unit testing
    /// </summary>
    Test,

    /// <summary>
    /// An unexpected or unrecognized error
    /// </summary>
    Unknown,

    /// <summary>
    /// The argument has the wrong type
    /// </summary>
    WrongParameterType,
    TypeNotComparable,

    CannotCreateScopedContext,
    CannotCreateGeneric,

    CannotInferType,

    UnitExpected,
    SCLSyntaxError,
    SingleCharacterExpected,

    SchemaViolationUnexpectedList,
    SchemaViolationUnexpectedNull,
    SchemaViolationUnmatchedRegex,
    SchemaViolationWrongType,
    SchemaViolationUnexpectedProperty,
    SchemaViolationMissingProperty,

    SchemaInvalidMissingEnum,
    SchemaInvalidNoEnumValues
}

}
