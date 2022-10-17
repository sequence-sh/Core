namespace Reductech.Sequence.Core.Internal;

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
    Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver,
        OptimizationSettings settings);

    /// <summary>
    /// Check that freezing this step is at least possible
    /// </summary>
    UnitResult<IError> CheckFreezePossible(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver);

    /// <summary>
    /// Gets the variables used by this step and its children and the types of those variables if they can be resolved at this time.
    /// Returns an error if the type name cannot be resolved
    /// </summary>
    Result<IReadOnlyCollection<UsedVariable>, IError>
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
    /// Move named arguments up a level if necessary
    /// </summary>
    IFreezableStep ReorganizeNamedArguments(StepFactoryStore stepFactoryStore);

    /// <summary>
    /// Tries to freeze this step.
    /// </summary>
    public Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        StepFactoryStore stepFactoryStore,
        OptimizationSettings optimizationSettings)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (stepFactoryStore is null) //This was happening in some situations. Possibly a race condition with the connector manager
        {
            return ErrorCode.Unknown.ToErrorBuilder("StepFactoryStore is null")
                .WithLocationSingle(ErrorLocation.EmptyLocation);
        }

        var thisReorganized = ReorganizeNamedArguments(stepFactoryStore);

        //var variables = optimizationSettings.InjectedVariables.GetValueOrDefault();

        var typeResolver = TypeResolver
            .TryCreate(
                stepFactoryStore,
                callerMetadata,
                Maybe<VariableName>.None,
                thisReorganized,
                optimizationSettings.InjectedVariables
            );

        if (typeResolver.IsFailure)
            return typeResolver.ConvertFailure<IStep>();

        //Check all variables are set
        var unsetVariableErrors =
            thisReorganized.GetVariablesUsed(callerMetadata, typeResolver.Value)
                .Value
                .GroupBy(x => x.VariableName)
                .Where(x => !x.Any(v => v.WasSet))
                .Where(x => !optimizationSettings.InjectedVariables.ContainsKey(x.Key))
                .SelectMany(
                    group =>
                        group.Select(
                            x =>
                                x.VariableName == VariableName.Item
                                    ? ErrorCode.AutomaticVariableNotSet.ToErrorBuilder()
                                        .WithLocation(x.Location)
                                    : ErrorCode.MissingVariable.ToErrorBuilder(x.VariableName)
                                        .WithLocation(x.Location)
                        )
                )
                .ToList();

        if (unsetVariableErrors.Any())
            return Result.Failure<IStep, IError>(ErrorList.Combine(unsetVariableErrors));

        var freezeResult = thisReorganized.TryFreeze(
            callerMetadata,
            typeResolver.Value,
            OptimizationSettings.None
        );

        return freezeResult;
    }
}
