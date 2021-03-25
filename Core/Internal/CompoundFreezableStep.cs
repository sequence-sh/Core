using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A step that is not a constant or a variable reference.
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed record CompoundFreezableStep(
        string StepName,
        FreezableStepData FreezableStepData,
        TextLocation? TextLocation) : IFreezableStep
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <summary>
    /// Try to get this step factory from the store.
    /// </summary>
    public Result<IStepFactory, IError> TryGetStepFactory(StepFactoryStore stepFactoryStore)
    {
        var r =
            stepFactoryStore.Dictionary.TryFindOrFail(
                StepName,
                () => ErrorHelper.MissingStepError(StepName)
                    .WithLocation(FreezableStepData)
            );

        return r;
    }

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(TypeReference expectedType, TypeResolver typeResolver)
    {
        return TryGetStepFactory(typeResolver.StepFactoryStore)
            .Bind(
                x =>
                    x.TryFreeze(expectedType, typeResolver, FreezableStepData)
            );
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<(VariableName variableName, TypeReference)>, IError>
        GetVariablesSet(TypeReference expectedType, TypeResolver typeResolver)
    {
        var stepFactory = TryGetStepFactory(typeResolver.StepFactoryStore);

        if (stepFactory.IsFailure)
            return stepFactory
                .ConvertFailure<IReadOnlyCollection<(VariableName variableName,
                    TypeReference)>>();

        var dataResult = FreezableStepData.GetVariablesSet(StepName, expectedType, typeResolver);

        if (dataResult.IsFailure)
            return dataResult;

        return dataResult.Value
            .Concat(
                stepFactory.Value.GetVariablesSet(expectedType, FreezableStepData, typeResolver)
            )
            .ToList();
    }

    /// <inheritdoc />
    public Result<TypeReference, IError> TryGetOutputTypeReference(
        TypeReference expectedType,
        TypeResolver typeResolver)
    {
        return TryGetStepFactory(typeResolver.StepFactoryStore)
            .Bind(x => x.TryGetOutputTypeReference(expectedType, FreezableStepData, typeResolver));
    }

    /// <inheritdoc />
    public override string ToString() => StepName;

    /// <inheritdoc />
    public bool Equals(IFreezableStep? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (other is CompoundFreezableStep fs)
        {
            return StepName.Equals(fs.StepName, StringComparison.OrdinalIgnoreCase) &&
                   FreezableStepData.Equals(fs.FreezableStepData);
        }

        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(
        StepName,
        FreezableStepData
    );
}

}
