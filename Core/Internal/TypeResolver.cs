using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Gets the actual type from a type reference.
/// </summary>
public sealed class TypeResolver
{
    /// <summary>
    /// Create a new TypeResolver
    /// </summary>
    private TypeResolver(
        StepFactoryStore stepFactoryStore,
        Maybe<VariableName> automaticVariableName,
        Dictionary<VariableName, TypeReference>? myDictionary = null)
    {
        StepFactoryStore      = stepFactoryStore;
        AutomaticVariableName = automaticVariableName;
        MyDictionary          = myDictionary ?? new Dictionary<VariableName, TypeReference>();
    }

    /// <summary>
    /// Copy this type resolver.
    /// </summary>
    public TypeResolver Copy()
    {
        var dictClone = MyDictionary.ToDictionary(x => x.Key, x => x.Value);
        return new TypeResolver(StepFactoryStore, AutomaticVariableName, dictClone);
    }

    /// <inheritdoc />
    public override string ToString() => Dictionary.Count + " Types";

    private Dictionary<VariableName, TypeReference> MyDictionary { get; }

    /// <summary>
    /// The dictionary mapping VariableNames to ActualTypeReferences
    /// </summary>
    public IReadOnlyDictionary<VariableName, TypeReference> Dictionary => MyDictionary;

    /// <summary>
    /// The name of the automatic variable
    /// </summary>
    public Maybe<VariableName> AutomaticVariableName { get; private set; }

    /// <summary>
    /// The StepFactoryStory
    /// </summary>
    public StepFactoryStore StepFactoryStore { get; }

    /// <summary>
    /// Try to Clone this context with additional set variables from a lambda function
    /// </summary>
    public Result<TypeResolver, IError> TryCloneWithScopedLambda(
        FreezableStepProperty.Lambda lambda,
        TypeReference typeReference,
        CallerMetadata scopedCallerMetadata)
    {
        var newTypeResolver = Copy();
        var vn              = lambda.VariableNameOrItem;

        var r1 = newTypeResolver.TryAddType(vn, typeReference);

        if (r1.IsFailure)
            return r1.ConvertFailure<TypeResolver>()
                .MapError(x => x.WithLocation(lambda.Location));

        var r2 = newTypeResolver.TryAddTypeHierarchy(scopedCallerMetadata, lambda.FreezableStep);

        if (r2.IsFailure)
            return r2.ConvertFailure<TypeResolver>();

        newTypeResolver.AutomaticVariableName = vn;

        return newTypeResolver;
    }

    /// <summary>
    /// Tries to create a new TypeResolver.
    /// </summary>
    public static Result<TypeResolver, IError> TryCreate(
        StepFactoryStore stepFactoryStore,
        CallerMetadata callerMetadata,
        Maybe<VariableName> automaticVariableName,
        IFreezableStep topLevelStep)
    {
        var typeResolver = new TypeResolver(stepFactoryStore, automaticVariableName);

        var r = typeResolver.TryAddTypeHierarchy(callerMetadata, topLevelStep);

        if (r.IsFailure)
            return r.ConvertFailure<TypeResolver>();

        return typeResolver;
    }

    /// <summary>
    /// Try to add this step and all its children to this TypeResolver.
    /// </summary>
    public Result<Unit, IError> TryAddTypeHierarchy(
        CallerMetadata callerMetadata,
        IFreezableStep topLevelStep)
    {
        int? numberUnresolved = null;

        while (true)
        {
            var unresolvableVariableNames = new List<VariableName>();
            var errors                    = new List<IError>();

            var result = topLevelStep.GetVariablesSet(callerMetadata, this);

            if (result.IsFailure)
                return result.ConvertFailure<Unit>();

            foreach (var (variableName, typeReference) in result.Value)
            {
                if (typeReference.IsUnknown)
                    unresolvableVariableNames.Add(variableName);
                else
                {
                    var addResult = TryAddType(variableName, typeReference);

                    if (addResult.IsFailure)
                        errors.Add(addResult.Error.WithLocation(topLevelStep.TextLocation));
                }
            }

            if (errors.Any())
                return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

            unresolvableVariableNames.RemoveAll(
                Dictionary.ContainsKey
            ); //remove all unresolvable variables that we have resolved some other way

            if (!unresolvableVariableNames.Any())
                break; //We've resolved everything. Yey!

            if (numberUnresolved == null || numberUnresolved > unresolvableVariableNames.Count)
                numberUnresolved =
                    unresolvableVariableNames
                        .Count; //We have improved this number and can try again
            else
            {
                var error =
                    ErrorList.Combine(
                        unresolvableVariableNames.Distinct()
                            .Select(
                                x =>
                                    ErrorCode.CouldNotResolveVariable.ToErrorBuilder(x.Name)
                                        .WithLocationSingle(ErrorLocation.EmptyLocation)
                            )
                    );

                return Result.Failure<Unit, IError>(error);
            }
        }

        return Unit.Default;
    }

    /// <summary>
    /// Tries to add a variableName with this type to the type resolver.
    /// </summary>
    public Result<Unit, IErrorBuilder> TryAddType(
        VariableName variable,
        TypeReference typeReference)
    {
        var can = CanAddType(variable, typeReference);

        if (can.IsSuccess)
        {
            if (can.Value)
                MyDictionary[variable] = typeReference;

            return Unit.Default;
        }

        return can.ConvertFailure<Unit>();
    }

    /// <summary>
    /// Determines whether a particular variable can be added to the type resolver.
    /// Returns true if it should be added, false if it is already present
    /// </summary>
    public Result<bool, IErrorBuilder> CanAddType(
        VariableName variable,
        TypeReference typeReference)
    {
        var actualType = typeReference.TryGetType(this);

        if (actualType.IsFailure)
            return actualType.ConvertFailure<bool>();

        if (MyDictionary.TryGetValue(variable, out var previous))
        {
            if (previous.Equals(typeReference)
             || typeReference.Allow(previous, this)) //The variable already had this type reference
                return false;

            if (!previous.Allow(
                typeReference,
                this
            ))
                return ErrorCode.WrongVariableType.ToErrorBuilder(
                    variable.Name,
                    typeReference.Name
                );
        }

        return true;
    }

    /// <summary>
    /// Resolve this type reference if it is variable and in the dictionary.
    /// </summary>
    public TypeReference? MaybeResolve(TypeReference typeReference)
    {
        if (typeReference is TypeReference.Variable vr
         && Dictionary.TryGetValue(vr.VariableName, out var tr))
            return tr;

        if (typeReference is TypeReference.AutomaticVariable && AutomaticVariableName.HasValue
                                                             && Dictionary.TryGetValue(
                                                                    AutomaticVariableName.Value,
                                                                    out var tr2
                                                                ))
            return tr2;

        return null;
    }
}

}
