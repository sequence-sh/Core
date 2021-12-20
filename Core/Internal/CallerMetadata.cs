namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// Metadata associated with the calling step
/// </summary>
public record CallerMetadata(string StepName, string ParameterName, TypeReference ExpectedType)
{
    /// <summary>
    /// Check that this property allows a particular type
    /// </summary>
    /// <returns></returns>
    public Result<Unit, IErrorBuilder> CheckAllows(
        TypeReference reference,
        TypeResolver? typeResolver)
    {
        if (ExpectedType.Allow(reference, typeResolver))
            return Unit.Default;

        return GetWrongTypeErrorBuilder("Step", reference.Name);
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
}
