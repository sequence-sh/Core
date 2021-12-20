namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A step factory that uses default values for most properties.
/// </summary>
public class SimpleStepFactory<TStep, TOutput> : StepFactory
    where TStep : ICompoundStep<TOutput>, new() where TOutput : ISCLObject
{
    /// <inheritdoc />
    public override Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver)
    {
        var reference = TypeReference.Create(typeof(TOutput));

        return callerMetadata
            .CheckAllows(reference, typeResolver)
            .MapError(x => x.WithLocation(freezableStepData))
            .Map(_ => reference);
    }

    /// <inheritdoc />
    public override Type StepType => typeof(TStep);

    /// <inheritdoc />
    protected override Result<ICompoundStep, IError> TryCreateInstance(
        CallerMetadata callerMetadata,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver)
    {
        var r = TryGetOutputTypeReference(callerMetadata, freezableStepData, typeResolver);

        if (r.IsFailure)
            return r.ConvertFailure<ICompoundStep>();

        return new TStep();
    }

    /// <inheritdoc />
    public override string OutputTypeExplanation => typeof(TOutput).Name.Replace("`1", "<T>");
}
