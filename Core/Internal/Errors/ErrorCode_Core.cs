using System.Diagnostics;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// Identifying code for an error message in Core
/// </summary>
public sealed record ErrorCode : ErrorCodeBase
{
    private ErrorCode(string code) : base(code) { }

    /// <inheritdoc />
    public override string GetFormatString()
    {
        var localizedMessage =
            ErrorMessages_EN.ResourceManager.GetString(Code); //TODO static method to get this

        Debug.Assert(localizedMessage != null, nameof(localizedMessage) + " != null");
        return localizedMessage;
    }

    /*
     * To Generate:
     * Replace ([^\t]+)\t([^\t]+)\t
     * With /// <summary>\r\n/// $2\r\n/// </summary>\r\npublic ErrorCode_Core $1 = new(nameof($1));\r\n
     */

    /// <summary>
    /// Assertion Failed '{0}'
    /// </summary>
    public static readonly ErrorCode AssertionFailed = new(nameof(AssertionFailed));

    /// <summary>
    /// Could not create an instance of {1}&lt;{0}&gt;
    /// </summary>
    public static readonly ErrorCode CannotCreateGeneric = new(nameof(CannotCreateGeneric));

    /// <summary>
    /// {0} cannot create a scoped context
    /// </summary>
    public static readonly ErrorCode CannotCreateScopedContext =
        new(nameof(CannotCreateScopedContext));

    /// <summary>
    /// Could not infer type
    /// </summary>
    public static readonly ErrorCode CannotInferType = new(nameof(CannotInferType));

    /// <summary>
    /// Parameters {0} and {1} are in conflict.
    /// </summary>
    public static readonly ErrorCode
        ConflictingParameters = new(nameof(ConflictingParameters));

    /// <summary>
    /// Could not parse '{0}' as {1}
    /// </summary>
    public static readonly ErrorCode CouldNotParse = new(nameof(CouldNotParse));

    /// <summary>
    /// Could not resolve variable '{0}'
    /// </summary>
    public static readonly ErrorCode CouldNotResolveVariable =
        new(nameof(CouldNotResolveVariable));

    /// <summary>
    /// Error Reading CSV
    /// </summary>
    public static readonly ErrorCode CSVError = new(nameof(CSVError));

    /// <summary>
    /// Attempt to Divide by Zero.
    /// </summary>
    public static readonly ErrorCode DivideByZero = new(nameof(DivideByZero));

    /// <summary>
    /// Duplicate Parameter: {0}.
    /// </summary>
    public static readonly ErrorCode DuplicateParameter = new(nameof(DuplicateParameter));

    /// <summary>
    /// SCL is empty.
    /// </summary>
    public static readonly ErrorCode EmptySequence = new(nameof(EmptySequence));

    /// <summary>
    /// External Process Failed: '{0}'
    /// </summary>
    public static readonly ErrorCode ExternalProcessError = new(nameof(ExternalProcessError));

    /// <summary>
    /// External process {0} did not return an output of the expected form
    /// </summary>
    public static readonly ErrorCode ExternalProcessMissingOutput =
        new(nameof(ExternalProcessMissingOutput));

    /// <summary>
    /// Could not find process '{0}'
    /// </summary>
    public static readonly ErrorCode ExternalProcessNotFound =
        new(nameof(ExternalProcessNotFound));

    /// <summary>
    /// Index was outside the bounds of the array.
    /// </summary>
    public static readonly ErrorCode IndexOutOfBounds = new(nameof(IndexOutOfBounds));

    /// <summary>
    /// '{0}' cannot take the value '{1}'
    /// </summary>
    public static readonly ErrorCode InvalidCast = new(nameof(InvalidCast));

    /// <summary>
    /// Could not get context '{0}'
    /// </summary>
    public static readonly ErrorCode MissingContext = new(nameof(MissingContext));

    /// <summary>
    /// {0} was missing or empty.
    /// </summary>
    public static readonly ErrorCode MissingParameter = new(nameof(MissingParameter));

    /// <summary>
    /// Could not get settings: {0}
    /// </summary>
    public static readonly ErrorCode MissingStepSettings = new(nameof(MissingStepSettings));

    /// <summary>
    /// Could not get settings value: {0}.{1}
    /// </summary>
    public static readonly ErrorCode MissingStepSettingsValue =
        new(nameof(MissingStepSettingsValue));

    /// <summary>
    /// Variable '{0}' does not exist.
    /// </summary>
    public static readonly ErrorCode MissingVariable = new(nameof(MissingVariable));

    /// <summary>
    /// Requirement '{0}' not met.
    /// </summary>
    public static readonly ErrorCode RequirementNotMet = new(nameof(RequirementNotMet));

    /// <summary>
    /// Schema Invalid: No Enum name defined
    /// </summary>
    public static readonly ErrorCode SchemaInvalidMissingEnum =
        new(nameof(SchemaInvalidMissingEnum));

    /// <summary>
    /// SchemaInvalid: No Enum values defined
    /// </summary>
    public static readonly ErrorCode SchemaInvalidNoEnumValues =
        new(nameof(SchemaInvalidNoEnumValues));

    /// <summary>
    /// Schema Violated: Missing Property: '{0}'
    /// </summary>
    public static readonly ErrorCode SchemaViolationMissingProperty =
        new(nameof(SchemaViolationMissingProperty));

    /// <summary>
    /// Schema Violated: Did not expect a list
    /// </summary>
    public static readonly ErrorCode SchemaViolationUnexpectedList =
        new(nameof(SchemaViolationUnexpectedList));

    /// <summary>
    /// Schema Violated: Expected not null
    /// </summary>
    public static readonly ErrorCode SchemaViolationUnexpectedNull =
        new(nameof(SchemaViolationUnexpectedNull));

    /// <summary>
    /// Schema Violated: Unexpected Property: '{0}'
    /// </summary>
    public static readonly ErrorCode SchemaViolationUnexpectedProperty =
        new(nameof(SchemaViolationUnexpectedProperty));

    /// <summary>
    /// Schema Violated: '{0}' does not match regex '{1}'
    /// </summary>
    public static readonly ErrorCode SchemaViolationUnmatchedRegex =
        new(nameof(SchemaViolationUnmatchedRegex));

    /// <summary>
    /// Schema Violated: '{0}' is not a {1}
    /// </summary>
    public static readonly ErrorCode SchemaViolationWrongType =
        new(nameof(SchemaViolationWrongType));

    /// <summary>
    /// Syntax Error: {0}
    /// </summary>
    public static readonly ErrorCode SCLSyntaxError = new(nameof(SCLSyntaxError));

    /// <summary>
    /// {0} should be a single character, but was '{1}'.
    /// </summary>
    public static readonly ErrorCode SingleCharacterExpected =
        new(nameof(SingleCharacterExpected));

    /// <summary>
    /// The step '{0}' does not exist
    /// </summary>
    public static readonly ErrorCode StepDoesNotExist = new(nameof(StepDoesNotExist));

    /// <summary>
    /// Test Error Message: '{0}'
    /// </summary>
    public static readonly ErrorCode Test = new(nameof(Test));

    /// <summary>
    /// Type {0} is not comparable and so cannot be used for sorting.
    /// </summary>
    public static readonly ErrorCode TypeNotComparable = new(nameof(TypeNotComparable));

    /// <summary>
    /// Enum '{0}' does not exist
    /// </summary>
    public static readonly ErrorCode UnexpectedEnumType = new(nameof(UnexpectedEnumType));

    /// <summary>
    /// Unexpected {0}: {1}
    /// </summary>
    public static readonly ErrorCode UnexpectedEnumValue = new(nameof(UnexpectedEnumValue));

    /// <summary>
    /// Unexpected Parameter '{0}' in '{1}'
    /// </summary>
    public static readonly ErrorCode UnexpectedParameter = new(nameof(UnexpectedParameter));

    /// <summary>
    /// An SCL Sequence should have a final return type of Unit. Try wrapping your sequence with 'Print'.
    /// </summary>
    public static readonly ErrorCode UnitExpected = new(nameof(UnitExpected));

    /// <summary>
    /// Unknown Error: '{0}'
    /// </summary>
    public static readonly ErrorCode Unknown = new(nameof(Unknown));

    /// <summary>
    /// {0} was a {1}, not a {2}
    /// </summary>
    public static readonly ErrorCode WrongParameterType = new(nameof(WrongParameterType));

    /// <summary>
    /// Variable '{0}' does not have type '{1}'.
    /// </summary>
    public static readonly ErrorCode WrongVariableType = new(nameof(WrongVariableType));

    /// <summary>
    /// Cannot convert a nested entity to {0}.
    /// </summary>
    public static readonly ErrorCode CannotConvertNestedEntity =
        new(nameof(CannotConvertNestedEntity));

    /// <summary>
    /// Cannot convert nested list to {0}.
    /// </summary>
    public static readonly ErrorCode CannotConvertNestedList =
        new(nameof(CannotConvertNestedList));

    /// <summary>
    /// Directory does not exist or could not be found: '{0}'
    /// </summary>
    public static readonly ErrorCode DirectoryNotFound = new(nameof(DirectoryNotFound));
}

}
