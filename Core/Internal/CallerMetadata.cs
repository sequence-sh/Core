using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Metadata associated with the calling step
/// </summary>
public record CallerMetadata(string StepName, string ParameterName, TypeReference ExpectedType)
{
    /// <summary>
    /// Gets the error thrown if the caller is given the wrong type
    /// </summary>
    /// <returns></returns>
    public SingleError GetWrongTypeError(
        string calledStep,
        string calledStepOutputType,
        ErrorLocation location)
    {
        var error = GetWrongTypeErrorBuilder(calledStep, calledStepOutputType)
            .WithLocationSingle(location);

        return error;
    }

    private ErrorBuilder GetWrongTypeErrorBuilder(
        string calledStep,
        string calledStepOutputType)
    {
        var error =
            ErrorCode.WrongType.ToErrorBuilder(
                StepName,
                ExpectedType.Name,
                ParameterName,
                calledStep,
                calledStepOutputType
            );

        return error;
    }

    /// <summary>
    /// Check that this property allows a particular type
    /// </summary>
    /// <returns></returns>
    public Result<Util.Unit, IErrorBuilder> CheckAllows(
        TypeReference reference,
        TypeResolver? typeResolver)
    {
        if (ExpectedType.Allow(reference, typeResolver))
            return Util.Unit.Default;

        return GetWrongTypeErrorBuilder("Step", reference.Name);
    }
}

}
