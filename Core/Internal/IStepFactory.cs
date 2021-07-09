using System;
using System.Collections.Generic;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A factory for creating steps.
/// </summary>
public interface IStepFactory
{
    /// <summary>
    /// Unique name for this type of step.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// The type of the step to create.
    /// </summary>
    public Type StepType { get; }

    /// <summary>
    /// The category of the step. Used for documentation.
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Dictionary mapping parameter references to properties.
    /// </summary>
    IReadOnlyDictionary<StepParameterReference, PropertyInfo> ParameterDictionary { get; }

    /// <summary>
    /// Tries to get a reference to the output type of this step.
    /// </summary>
    Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        FreezableStepData freezeData,
        TypeResolver typeResolver);

    /// <summary>
    /// Gets all type references set by this method and their values if they can be calculated.
    /// </summary>
    IEnumerable<(VariableName variableName, TypeReference type)> GetVariablesUsed(
        CallerMetadata callerMetadata,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver);

    /// <summary>
    /// Serializer to use for serialization.
    /// </summary>
    IStepSerializer Serializer { get; }

    /// <summary>
    /// Special requirements for this step.
    /// </summary>
    IEnumerable<Requirement> Requirements { get; }

    /// <summary>
    /// Try to create the instance of this type and set all arguments.
    /// </summary>
    Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver,
        FreezableStepData freezeData);

    /// <summary>
    /// Human readable explanation of the output type.
    /// </summary>
    string OutputTypeExplanation { get; }

    /// <summary>
    /// Gets all enum types used by this step.
    /// </summary>
    IEnumerable<Type> EnumTypes { get; }
}

}
