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
public sealed record CompoundFreezableStep(
    string StepName,
    FreezableStepData FreezableStepData,
    TextLocation TextLocation) : IFreezableStep
{
    /// <summary>
    /// Try to get this step factory from the store.
    /// </summary>
    public Result<IStepFactory, IError> TryGetStepFactory(StepFactoryStore stepFactoryStore)
    {
        var r =
            stepFactoryStore.Dictionary.TryFindOrFail(
                StepName,
                () => ErrorCode.StepDoesNotExist.ToErrorBuilder(StepName)
                    .WithLocation(FreezableStepData)
            );

        return r;
    }

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        return TryGetStepFactory(typeResolver.StepFactoryStore)
            .Bind(
                x =>
                    x.TryFreeze(callerMetadata, typeResolver, FreezableStepData)
            );
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<UsedVariable>,
            IError>
        GetVariablesUsed(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        var stepFactory = TryGetStepFactory(typeResolver.StepFactoryStore);

        if (stepFactory.IsFailure)
            return stepFactory
                .ConvertFailure<IReadOnlyCollection<UsedVariable>>();

        var dataResult = FreezableStepData.GetVariablesUsed(StepName, callerMetadata, typeResolver);

        if (dataResult.IsFailure)
            return dataResult;

        var sfResult = stepFactory.Value
            .GetVariablesUsed(callerMetadata, FreezableStepData, typeResolver)
            .ToList();

        return dataResult.Value.Concat(sfResult).ToList();
    }

    /// <inheritdoc />
    public Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        return TryGetStepFactory(typeResolver.StepFactoryStore)
            .Bind(
                x => x.TryGetOutputTypeReference(callerMetadata, FreezableStepData, typeResolver)
            );
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

        if (other is CompoundFreezableStep(var stepName, var freezableStepData, _))
        {
            return StepName.Equals(stepName, StringComparison.OrdinalIgnoreCase) &&
                   FreezableStepData.Equals(freezableStepData);
        }

        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(
        StepName,
        FreezableStepData
    );

    /// <inheritdoc />
    public Result<IFreezableStep, IError> ReorganizeNamedArguments(
        StepFactoryStore stepFactoryStore)
    {
        var dict   = new Dictionary<StepParameterReference, FreezableStepProperty>();
        var errors = new List<IError>();

        foreach (var (key, value) in FreezableStepData.StepProperties)
        {
            var r = value.ReorganizeNamedArguments(stepFactoryStore);

            if (r.IsFailure)
                errors.Add(r.Error);
            else if (
                !r.Value.StepMetadata.Bracketed &&
                !r.Value.StepMetadata.PassedAsInfix &&
                r.Value is FreezableStepProperty.Step(
                    CompoundFreezableStep(var stepName, var newStepData, var innerTextLocation), var
                    outerTextLocation) &&
                stepFactoryStore.Dictionary.TryGetValue(stepName, out var stepFactory)
            )
            {
                var nestedDict = new Dictionary<StepParameterReference, FreezableStepProperty>();
                var changed    = false;

                foreach (var (stepParameterReference, freezableStepProperty) in newStepData
                    .StepProperties)
                {
                    if (stepParameterReference is StepParameterReference.Named
                     && !stepFactory.ParameterDictionary.ContainsKey(stepParameterReference))
                    {
                        dict.Add(stepParameterReference, freezableStepProperty);
                    }
                    else
                    {
                        nestedDict.Add(stepParameterReference, freezableStepProperty);
                        changed = true;
                    }
                }

                var newStep = changed
                    ? new FreezableStepProperty.Step(
                        new CompoundFreezableStep(
                            stepName,
                            newStepData with { StepProperties = nestedDict },
                            innerTextLocation
                        ),
                        outerTextLocation
                    )
                    : r.Value;

                dict.Add(key, newStep);
            }
            else
            {
                dict.Add(key, r.Value);
            }
        }

        if (errors.Any())
            return Result.Failure<IFreezableStep, IError>(ErrorList.Combine(errors));

        return this with { FreezableStepData = FreezableStepData with { StepProperties = dict } };
    }
}

}
