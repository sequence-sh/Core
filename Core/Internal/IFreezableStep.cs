using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A step which can be frozen.
/// </summary>
public interface IFreezableStep : IEquatable<IFreezableStep>
{
    /// <summary>
    /// The human-readable name of this step.
    /// </summary>
    string StepName { get; }

    /// <summary>
    /// The SCL text location where this step comes from
    /// </summary>
    public TextLocation TextLocation { get; }

    /// <summary>
    /// Try to freeze this step.
    /// </summary>
    Result<IStep, IError> TryFreeze(CallerMetadata callerMetadata, TypeResolver typeResolver);

    /// <summary>
    /// Gets the variables used by this step and its children and the types of those variables if they can be resolved at this time.
    /// Returns an error if the type name cannot be resolved
    /// </summary>
    Result<IReadOnlyCollection<(VariableName variableName, TypeReference typeReference)>, IError>
        GetVariablesUsed(
            CallerMetadata callerMetadata,
            TypeResolver typeResolver);

    /// <summary>
    /// The output type of this step. Will be unit if the step does not have an output.
    /// </summary>
    Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver);

    /// <summary>
    /// Tries to freeze this step.
    /// </summary>
    public Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        StepFactoryStore stepFactoryStore) => TypeResolver
        .TryCreate(stepFactoryStore, callerMetadata, Maybe<VariableName>.None, this)
        .Bind(typeResolver => TryFreeze(callerMetadata, typeResolver));
}

}
