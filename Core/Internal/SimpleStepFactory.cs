using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A step factory that uses default values for most properties.
/// </summary>
public class SimpleStepFactory<TStep, TOutput> : StepFactory
    where TStep : ICompoundStep<TOutput>, new()
{
    /// <inheritdoc />
    public override Result<TypeReference, IError> TryGetOutputTypeReference(
        TypeReference expectedTypeReference,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver)
    {
        var reference = TypeReference.Create(typeof(TOutput));

        return expectedTypeReference
            .CheckAllows(reference, StepType)
            .MapError(x => x.WithLocation(freezableStepData))
            .Map(_ => reference);
    }

    /// <inheritdoc />
    public override Type StepType => typeof(TStep);

    /// <inheritdoc />
    protected override Result<ICompoundStep, IError> TryCreateInstance(
        TypeReference expectedTypeReference,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver)
    {
        var r = TryGetOutputTypeReference(expectedTypeReference, freezableStepData, typeResolver);

        if (r.IsFailure)
            return r.ConvertFailure<ICompoundStep>();

        return new TStep();
    }

    /// <inheritdoc />
    public override string OutputTypeExplanation => typeof(TOutput).Name.Replace("`1", "<T>");
}

}
