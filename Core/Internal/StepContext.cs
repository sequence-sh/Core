using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Keeps track of all variables in a Freezable context.
/// </summary>
public sealed class StepContext //TODO remove this type and move all logic to TypeResolver
{
    /// <summary>
    /// Dictionary mapping variable names to types.
    /// </summary>
    public TypeResolver TypeResolver { get; }

    private StepContext(TypeResolver typeResolver) => TypeResolver = typeResolver;

    /// <summary>
    /// Try to Clone this context with additional set variables.
    /// </summary>
    public Result<StepContext, IError> TryCloneWithScopedStep(
        VariableName vn,
        ITypeReference typeReference,
        IFreezableStep scopedStep,
        IErrorLocation errorLocation)
    {
        var newTypeResolver = TypeResolver.Copy();

        var r1 = newTypeResolver.TryAddType(vn, typeReference);

        if (r1.IsFailure)
            return r1.ConvertFailure<StepContext>().MapError(x => x.WithLocation(errorLocation));

        var r2 = newTypeResolver.TryAddTypeHierarchy(scopedStep);

        if (r2.IsFailure)
            return r2.ConvertFailure<StepContext>();

        return new StepContext(newTypeResolver);
    }

    /// <summary>
    /// Gets the type referred to by a reference.
    /// </summary>
    public Result<Type, IErrorBuilder> TryGetTypeFromReference(ITypeReference typeReference) =>
        typeReference.TryGetActualTypeReference(TypeResolver).Map(x => x.Type);

    /// <summary>
    /// Tries to create a new StepContext.
    /// </summary>
    public static Result<StepContext, IError> TryCreate(
        StepFactoryStore stepFactoryStore,
        IFreezableStep topLevelStep)
    {
        var typeResolver = new TypeResolver(stepFactoryStore);

        var r = typeResolver.TryAddTypeHierarchy(topLevelStep);

        if (r.IsFailure)
            return r.ConvertFailure<StepContext>();

        return new StepContext(typeResolver);
    }
}

}
