namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Identifying code for an error message.
/// </summary>
public enum ErrorCode
{
    /*
     * To Generate:
     * Replace ([^\t]+)\t([^\t]+)\t
     * With /// <summary>\r\n/// $2\r\n/// </summary>\r\n$1,\r\n
     */

    /// <summary>
    /// Assertion Failed '{0}'
    /// </summary>
    AssertionFailed,

    /// <summary>
    /// Could not create an instance of {1}&lt;{0}&gt;
    /// </summary>
    CannotCreateGeneric,

    /// <summary>
    /// {0} cannot create a scoped context
    /// </summary>
    CannotCreateScopedContext,

    /// <summary>
    /// Could not infer type
    /// </summary>
    CannotInferType,

    /// <summary>
    /// Parameters {0} and {1} are in conflict.
    /// </summary>
    ConflictingParameters,

    /// <summary>
    /// Could not parse '{0}' as {1}
    /// </summary>
    CouldNotParse,

    /// <summary>
    /// Could not resolve variable '{0}'
    /// </summary>
    CouldNotResolveVariable,

    /// <summary>
    /// Attempt to Divide by Zero.
    /// </summary>
    DivideByZero,

    /// <summary>
    /// Duplicate Parameter: {0}.
    /// </summary>
    DuplicateParameter,

    /// <summary>
    /// SCL is empty.
    /// </summary>
    EmptySequence,

    /// <summary>
    /// External Process Failed: '{0}'
    /// </summary>
    ExternalProcessError,

    /// <summary>
    /// External process {0} did not return an output of the expected form
    /// </summary>
    ExternalProcessMissingOutput,

    /// <summary>
    /// Could not find process '{0}'
    /// </summary>
    ExternalProcessNotFound,

    /// <summary>
    /// Index was outside the bounds of the array.
    /// </summary>
    IndexOutOfBounds,

    /// <summary>
    /// '{0}' cannot take the value '{1}'
    /// </summary>
    InvalidCast,

    /// <summary>
    /// {0} was missing or empty.
    /// </summary>
    MissingParameter,

    /// <summary>
    /// Could not get settings: {0}
    /// </summary>
    MissingStepSettings,

    /// <summary>
    /// Variable '{0}' does not exist.
    /// </summary>
    MissingVariable,

    /// <summary>
    /// Requirement '{0}' not met.
    /// </summary>
    RequirementNotMet,

    /// <summary>
    /// Schema Invalid: No Enum name defined
    /// </summary>
    SchemaInvalidMissingEnum,

    /// <summary>
    /// SchemaInvalid: No Enum values defined
    /// </summary>
    SchemaInvalidNoEnumValues,

    /// <summary>
    /// Schema Violated: Missing Property: '{0}'
    /// </summary>
    SchemaViolationMissingProperty,

    /// <summary>
    /// Schema Violated: Did not expect a list
    /// </summary>
    SchemaViolationUnexpectedList,

    /// <summary>
    /// Schema Violated: Expected not null
    /// </summary>
    SchemaViolationUnexpectedNull,

    /// <summary>
    /// Schema Violated: Unexpected Property: '{0}'
    /// </summary>
    SchemaViolationUnexpectedProperty,

    /// <summary>
    /// Schema Violated: '{0}' does not match regex '{1}'
    /// </summary>
    SchemaViolationUnmatchedRegex,

    /// <summary>
    /// Schema Violated: '{0}' is not a {1}
    /// </summary>
    SchemaViolationWrongType,

    /// <summary>
    /// Syntax Error: {0}
    /// </summary>
    SCLSyntaxError,

    /// <summary>
    /// {0} should be a single character, but was '{1}'.
    /// </summary>
    SingleCharacterExpected,

    /// <summary>
    /// The step '{0}' does not exist
    /// </summary>
    StepDoesNotExist,

    /// <summary>
    /// Test Error Message: '{0}'
    /// </summary>
    Test,

    /// <summary>
    /// Type {0} is not comparable and so cannot be used for sorting.
    /// </summary>
    TypeNotComparable,

    /// <summary>
    /// Enum '{0}' does not exist
    /// </summary>
    UnexpectedEnumType,

    /// <summary>
    /// Unexpected {0}: {1}
    /// </summary>
    UnexpectedEnumValue,

    /// <summary>
    /// Unexpected Parameter '{0}' in '{1}'
    /// </summary>
    UnexpectedParameter,

    /// <summary>
    /// An SCL Sequence should have a final return type of Unit. Try wrapping your sequence with 'Print'.
    /// </summary>
    UnitExpected,

    /// <summary>
    /// Unknown Error: '{0}'
    /// </summary>
    Unknown,

    /// <summary>
    /// {0} was a {1}, not a {2}
    /// </summary>
    WrongParameterType,

    /// <summary>
    /// Variable '{0}' does not have type '{1}'.
    /// </summary>
    WrongVariableType,
}

}
