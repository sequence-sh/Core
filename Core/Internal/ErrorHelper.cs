using JetBrains.Annotations;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Easy access to common errors.
/// </summary>
public static class ErrorHelper
{
    /// <summary>
    /// The error that should be returned when a step is requested which does not exist.
    /// </summary>
    [Pure]
    public static IErrorBuilder MissingStepError(string stepName) => new ErrorBuilder(
        ErrorCode_Core.StepDoesNotExist,
        stepName
    );

    /// <summary>
    /// The error that should be returned when a parameter is missing.
    /// </summary>
    [Pure]
    public static IErrorBuilder MissingParameterError(string parameterName) =>
        new ErrorBuilder(
            ErrorCode_Core.MissingParameter,
            parameterName
        );

    /// <summary>
    /// The error that should be returned when a parameter has the wrong type.
    /// </summary>
    [Pure]
    public static IErrorBuilder WrongParameterTypeError(
        string propertyName,
        MemberType realType,
        MemberType expectedType) => new ErrorBuilder(
        ErrorCode_Core.WrongParameterType,
        propertyName,
        realType,
        expectedType
    );

    /// <summary>
    /// The error that should be returned when a parameter is missing.
    /// </summary>
    [Pure]
    public static IErrorBuilder UnexpectedParameterError(string parameterName, string stepType) =>
        new ErrorBuilder(ErrorCode_Core.UnexpectedParameter, parameterName, stepType);
}

}
