namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// Freezes into a create entity step
/// </summary>
public record CreateEntityFreezableStep(FreezableEntityData FreezableEntityData) : IFreezableStep
{
    /// <inheritdoc />
    public bool Equals(IFreezableStep? other) => other is CreateEntityFreezableStep oStep
                                              && FreezableEntityData.Equals(
                                                     oStep.FreezableEntityData
                                                 );

    /// <inheritdoc />
    public string StepName => "Create Entity";

    /// <inheritdoc />
    public TextLocation TextLocation => FreezableEntityData.Location;

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        var checkResult = callerMetadata.CheckAllows(
                TypeReference.Actual.Entity,
                typeResolver
            )
            .MapError(x => x.WithLocation(this));

        if (checkResult.IsFailure)
            return checkResult.ConvertFailure<IStep>();

        var results = new List<Result<(EntityPropertyKey name, IStep value), IError>>();

        foreach (var (propertyName, stepMember) in FreezableEntityData.EntityProperties)
        {
            var cm = new CallerMetadata(
                StepName,
                propertyName.AsString,
                TypeReference.Any.Instance
            );

            var frozen = stepMember.ConvertToStep()
                .TryFreeze(cm, typeResolver)
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

        return new CreateEntityStep(r.Value) { TextLocation = TextLocation };
    }

    /// <inheritdoc />
    public UnitResult<IError> CheckFreezePossible(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        var checkResult = callerMetadata.CheckAllows(
                TypeReference.Actual.Entity,
                typeResolver
            )
            .MapError(x => x.WithLocation(this));

        if (checkResult.IsFailure)
            return checkResult.ConvertFailure<Unit>();

        var results = new List<UnitResult<IError>>();

        foreach (var (propertyName, stepMember) in FreezableEntityData.EntityProperties)
        {
            var cm = new CallerMetadata(
                StepName,
                propertyName.AsString,
                TypeReference.Any.Instance
            );

            var result = stepMember.ConvertToStep()
                .CheckFreezePossible(cm, typeResolver);

            results.Add(result);
        }

        if (results.All(x => x.IsSuccess))
            return UnitResult.Success<IError>();

        return UnitResult.Failure(
            ErrorList.Combine(results.Where(x => x.IsFailure).Select(x => x.Error))
        );
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<UsedVariable>,
            IError>
        GetVariablesUsed(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        return FreezableEntityData.GetVariablesUsed(callerMetadata, typeResolver);
    }

    /// <inheritdoc />
    public Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver) => TypeReference.Actual.Entity;

    /// <inheritdoc />
    public IFreezableStep ReorganizeNamedArguments(StepFactoryStore stepFactoryStore)
    {
        var dict = new Dictionary<EntityPropertyKey, FreezableStepProperty>();

        foreach (var (key, value) in FreezableEntityData.EntityProperties)
        {
            var r = value.ReorganizeNamedArguments(stepFactoryStore);
            dict.Add(key, r);
        }

        return this with
        {
            FreezableEntityData = FreezableEntityData with { EntityProperties = dict }
        };
    }
}
