﻿using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Freezes into a create entity step
/// </summary>
public record CreateEntityFreezableStep(
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        FreezableEntityData FreezableEntityData) : IFreezableStep
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <inheritdoc />
    public bool Equals(IFreezableStep? other) => other is CreateEntityFreezableStep oStep
                                              && FreezableEntityData.Equals(
                                                     oStep.FreezableEntityData
                                                 );

    /// <inheritdoc />
    public string StepName => "Create Entity";

    /// <inheritdoc />
    public TextLocation? TextLocation => FreezableEntityData.Location;

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(TypeResolver typeResolver)
    {
        var results = new List<Result<(EntityPropertyKey name, IStep value), IError>>();

        foreach (var (propertyName, stepMember) in FreezableEntityData.EntityProperties)
        {
            var frozen = stepMember.ConvertToStep()
                .TryFreeze(typeResolver)
                .Map(s => (propertyName, s));

            results.Add(frozen);
        }

        var r =
            results.Combine(ErrorList.Combine)
                .Map(
                    v =>
                        v.ToDictionary(x => x.name, x => x.value)
                );

        if (r.IsFailure)
            return r.ConvertFailure<IStep>();

        return new CreateEntityStep(r.Value);
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError>
        GetVariablesSet(TypeResolver typeResolver)
    {
        return FreezableEntityData.GetVariablesSet(typeResolver);
    }

    /// <inheritdoc />
    public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver) =>
        new ActualTypeReference(typeof(Entity));
}

}
