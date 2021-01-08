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
    public TypeResolver(
        StepFactoryStore stepFactoryStore,
        Dictionary<VariableName, ActualTypeReference>? myDictionary = null)
    {
        StepFactoryStore = stepFactoryStore;
        MyDictionary     = myDictionary ?? new Dictionary<VariableName, ActualTypeReference>();
    }

    /// <summary>
    /// Copy this type resolver.
    /// </summary>
    public TypeResolver Copy()
    {
        var dictClone = MyDictionary.ToDictionary(x => x.Key, x => x.Value);
        return new TypeResolver(StepFactoryStore, dictClone);
    }

    /// <inheritdoc />
    public override string ToString() => Dictionary.Count + " Types";

    private Dictionary<VariableName, ActualTypeReference> MyDictionary { get; }

    /// <summary>
    /// The dictionary mapping VariableNames to ActualTypeReferences
    /// </summary>
    public IReadOnlyDictionary<VariableName, ActualTypeReference> Dictionary => MyDictionary;

    /// <summary>
    /// The StepFactoryStory
    /// </summary>
    public StepFactoryStore StepFactoryStore { get; }

    /// <summary>
    /// Try to add this step and all its children to this TypeResolver.
    /// </summary>
    public Result<Unit, IError> TryAddTypeHierarchy(IFreezableStep topLevelStep)
    {
        int? numberUnresolved = null;

        while (true)
        {
            var unresolvableVariableNames = new List<VariableName>();
            var errors                    = new List<IError>();

            var result = topLevelStep.GetVariablesSet(this);

            if (result.IsFailure)
                return result.ConvertFailure<Unit>();

            foreach (var (variableName, maybe) in result.Value)
            {
                if (maybe.HasNoValue)
                    unresolvableVariableNames.Add(variableName);
                else
                {
                    var addResult = TryAddType(variableName, maybe.Value);

                    if (addResult.IsFailure)
                        errors.Add(addResult.Error.WithLocation(EntireSequenceLocation.Instance));
                    else if (!addResult.Value)
                        unresolvableVariableNames.Add(variableName);
                }
            }

            if (errors.Any())
                return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

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
                                    new SingleError_Core(
                                        EntireSequenceLocation.Instance,
                                        ErrorCode_Core.CouldNotResolveVariable,
                                        x.Name
                                    )
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
    public Result<bool, IErrorBuilder> TryAddType(
        VariableName variable,
        ITypeReference typeReference)
    {
        var actualType = typeReference.GetActualTypeReferenceIfResolvable(this);

        if (actualType.IsFailure)
            return actualType.ConvertFailure<bool>();

        if (actualType.Value.HasNoValue)
            return false;

        var actualTypeReference = actualType.Value.Value;

        if (MyDictionary.TryGetValue(variable, out var previous))
        {
            if (previous.Equals(actualTypeReference))
                return true;

            return new ErrorBuilder_Core(ErrorCode_Core.CannotInferType);
        }

        MyDictionary.Add(variable, actualTypeReference);
        return true;
    }
}

}
