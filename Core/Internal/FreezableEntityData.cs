using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// The data used by a Freezable Step.
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public record FreezableEntityData(
        IReadOnlyDictionary<EntityPropertyKey, FreezableStepProperty> EntityProperties,
        TextLocation? Location)
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <inheritdoc />
    public override string ToString()
    {
        var keys      = EntityProperties.OrderBy(x => x);
        var keyString = string.Join("; ", keys);

        if (string.IsNullOrWhiteSpace(keyString))
            return "Empty";

        return keyString;
    }

    /// <inheritdoc />
    public virtual bool Equals(FreezableEntityData? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        var result = DictionariesEqual1(EntityProperties, other.EntityProperties);

        return result;

        static bool DictionariesEqual1(
            IReadOnlyDictionary<EntityPropertyKey, FreezableStepProperty> dict1,
            IReadOnlyDictionary<EntityPropertyKey, FreezableStepProperty> dict2)
        {
            if (dict1.Count != dict2.Count)
                return false;

            foreach (var key in dict1.Keys)
            {
                if (!dict2.ContainsKey(key))
                    return false;

                if (!dict1[key].Equals(dict2[key]))
                    return false;
            }

            return true;
        }
    }

    /// <inheritdoc />
    public override int GetHashCode() => EntityProperties.Count;

    /// <summary>
    /// Gets the variables set by steps in this FreezableStepData.
    /// </summary>
    public Result<IReadOnlyCollection<(VariableName variableName, TypeReference)>, IError>
        GetVariablesSet(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        var variables = new List<(VariableName variableName, TypeReference)>();
        var errors    = new List<IError>();

        foreach (var stepProperty in EntityProperties)
        {
            switch (stepProperty.Value)
            {
                case FreezableStepProperty.Step step:
                    LocalGetVariablesSet(step.FreezableStep);
                    break;
                case FreezableStepProperty.StepList stepList:
                {
                    foreach (var step in stepList.List)
                        LocalGetVariablesSet(step);

                    break;
                }
                case FreezableStepProperty.Variable _: break;
                default:                               throw new ArgumentOutOfRangeException();
            }
        }

        if (errors.Any())
            return Result
                .Failure<IReadOnlyCollection<(VariableName variableName, TypeReference)>,
                    IError>(ErrorList.Combine(errors));

        return variables;

        void LocalGetVariablesSet(IFreezableStep freezableStep)
        {
            var variablesSet = freezableStep.GetVariablesSet(callerMetadata, typeResolver);

            if (variablesSet.IsFailure)
                errors.Add(variablesSet.Error);

            else
                variables.AddRange(variablesSet.Value);
        }
    }
}

}
