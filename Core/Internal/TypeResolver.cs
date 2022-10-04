namespace Reductech.Sequence.Core.Internal;

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
        Dictionary<VariableName, VariableReference>? myDictionary = null)
    {
        StepFactoryStore      = stepFactoryStore;
        AutomaticVariableName = automaticVariableName;
        MyDictionary          = myDictionary ?? new Dictionary<VariableName, VariableReference>();
    }

    /// <summary>
    /// A type resolver with no types
    /// </summary>
    public static TypeResolver Create(StepFactoryStore stepFactoryStore)
    {
        return new TypeResolver(stepFactoryStore, Maybe<VariableName>.None);
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

    private Dictionary<VariableName, VariableReference> MyDictionary { get; }

    /// <summary>
    /// The dictionary mapping VariableNames to ActualTypeReferences
    /// </summary>
    public IReadOnlyDictionary<VariableName, VariableReference> Dictionary => MyDictionary;

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
        newTypeResolver.AutomaticVariableName = vn;

        var r1 = newTypeResolver.TryAddType(
            vn,
            true,
            new VariableReference(
                typeReference,
                false,
                $"{scopedCallerMetadata.ParameterName} from {scopedCallerMetadata.StepName}",
                null
            )
        );

        if (r1.IsFailure)
        {
            if (r1.Error.GetErrorBuilders().Count() != 1
             || r1.Error.GetErrorBuilders().Single().ErrorCode != ErrorCode.WrongVariableType)
                return r1.ConvertFailure<TypeResolver>()
                    .MapError(x => x.WithLocation(lambda.Location));

            var r3 = lambda.FreezableStep.TryFreeze(scopedCallerMetadata, newTypeResolver);

            if (r3.IsFailure)
                return r3.ConvertFailure<TypeResolver>();

            return r1.ConvertFailure<TypeResolver>()
                .MapError(x => x.WithLocation(lambda.Location));
        }

        var r2 = newTypeResolver.TryAddTypeHierarchy(scopedCallerMetadata, lambda.FreezableStep);

        if (r2.IsFailure)
            return r2.ConvertFailure<TypeResolver>();

        return newTypeResolver;
    }

    /// <summary>
    /// Tries to create a new TypeResolver.
    /// </summary>
    public static Result<TypeResolver, IError> TryCreate(
        StepFactoryStore stepFactoryStore,
        CallerMetadata callerMetadata,
        Maybe<VariableName> automaticVariableName,
        IFreezableStep? topLevelStep,
        IReadOnlyDictionary<VariableName, InjectedVariable>? variablesToInject)
    {
        var typeResolver = new TypeResolver(stepFactoryStore, automaticVariableName);

        foreach (var (key, (value, description)) in variablesToInject
                                                 ?? ImmutableDictionary<VariableName,
                                                        InjectedVariable>.Empty)
        {
            var addResult = typeResolver.TryAddType(
                key,
                true,
                new VariableReference(
                    value.GetTypeReference(),
                    true,
                    description,
                    value.Serialize(SerializeOptions.Serialize)
                )
            );

            if (addResult.IsFailure)
                return addResult.ConvertFailure<TypeResolver>()
                    .MapError(x => x.WithLocation(ErrorLocation.EmptyLocation));
        }

        if (topLevelStep is not null)
        {
            var r = typeResolver.TryAddTypeHierarchy(callerMetadata, topLevelStep);

            if (r.IsFailure)
                return r.ConvertFailure<TypeResolver>();
        }

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
            var unresolvableVariableNames = new List<UsedVariable>();
            var errors                    = new List<IError>();

            var result = topLevelStep.GetVariablesUsed(callerMetadata, this);

            if (result.IsFailure)
                return result.ConvertFailure<Unit>();

            foreach (var usedVariable in result.Value)
            {
                if (usedVariable.TypeReference.IsUnknown)
                    unresolvableVariableNames.Add(usedVariable);
                else
                {
                    var addResult = TryAddType(
                        usedVariable.VariableName,
                        usedVariable.WasSet,
                        new VariableReference(usedVariable.TypeReference, false, null, null)
                    );

                    if (addResult.IsFailure)
                        errors.Add(addResult.Error.WithLocation(usedVariable.Location));
                }
            }

            if (errors.Any())
                return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

            foreach (var usedVariable in unresolvableVariableNames.ToList())
            {
                if (Dictionary.TryGetValue(usedVariable.VariableName, out var resolvedType))
                {
                    unresolvableVariableNames.Remove(usedVariable);

                    var combinedTypeResult = resolvedType.TypeReference.TryCombine(
                        usedVariable.TypeReference,
                        this
                    );

                    if (combinedTypeResult.IsFailure)
                        errors.Add(combinedTypeResult.Error.WithLocation(usedVariable.Location));
                    else if (combinedTypeResult.Value != resolvedType.TypeReference)
                    {
                        var addResult = TryAddType(
                            usedVariable.VariableName,
                            usedVariable.WasSet,
                            resolvedType with { TypeReference = combinedTypeResult.Value }
                        );

                        if (addResult.IsFailure)
                            errors.Add(addResult.Error.WithLocation(usedVariable.Location));
                    }
                }
            }

            if (!errors.Any())
            {
                if (!unresolvableVariableNames.Any())
                    break;

                if (Dictionary.Any() && (numberUnresolved is null
                                      || numberUnresolved > unresolvableVariableNames.Count))
                {
                    numberUnresolved =
                        unresolvableVariableNames
                            .Count; //We have improved this number and can try again

                    continue;
                }
            }

            errors.AddRange(
                unresolvableVariableNames.Distinct()
                    .Select(
                        x =>
                            ErrorCode.CouldNotResolveVariable
                                .ToErrorBuilder(x.VariableName.Name)
                                .WithLocationSingle(x.Location)
                    )
            );

            return Result.Failure<Unit, IError>(ErrorList.Combine(errors));
        }

        return Unit.Default;
    }

    /// <summary>
    /// Tries to add a variableName with this type to the type resolver.
    /// </summary>
    public Result<Unit, IErrorBuilder> TryAddType(
        VariableName variable,
        bool isBeingSet,
        VariableReference variableReference)
    {
        var can = CanAddType(variable, isBeingSet, variableReference.TypeReference);

        if (can.IsSuccess)
        {
            if (can.Value)
                MyDictionary[variable] = variableReference;

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
        bool isBeingSet,
        TypeReference typeReference)
    {
        if (MyDictionary.TryGetValue(variable, out var previous))
        {
            if (isBeingSet && previous.Injected)
            {
                return ErrorCode.AttemptToSetInjectedVariable.ToErrorBuilder(variable.Name);
            }

            if (previous.TypeReference.Equals(typeReference)
             || typeReference.Allow(
                    previous.TypeReference,
                    this
                )) //The variable already had this type reference
                return false;

            if (!previous.TypeReference.Allow(typeReference, this))
                return ErrorCode.WrongVariableType.ToErrorBuilder(
                    variable.Name,
                    typeReference.Name
                );
        }

        var actualType = typeReference.TryGetType(this);

        if (actualType.IsFailure)
            return actualType.ConvertFailure<bool>();

        return true;
    }

    /// <summary>
    /// Resolve this type reference if it is variable and in the dictionary.
    /// </summary>
    public TypeReference MaybeResolve(TypeReference typeReference)
    {
        while (true)
        {
            switch (typeReference)
            {
                case TypeReference.Variable vr
                    when Dictionary.TryGetValue(vr.VariableName, out var tr):
                    typeReference = tr.TypeReference;
                    continue;
                case TypeReference.AutomaticVariable when AutomaticVariableName.HasValue
                                                       && Dictionary.TryGetValue(
                                                              AutomaticVariableName
                                                                  .GetValueOrThrow(),
                                                              out var tr2
                                                          ):
                    typeReference = tr2.TypeReference;
                    continue;
                default: return typeReference;
            }
        }
    }
}
