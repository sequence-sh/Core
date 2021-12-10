namespace Reductech.EDR.Core.Internal;

/// <summary>
/// Null constant
/// </summary>
public class NullConstant : IConstantStep, IConstantFreezableStep, IStep<SCLNull>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public NullConstant(TextLocation textLocation)
    {
        TextLocation = textLocation;
    }

    /// <inheritdoc />
    public string Name => "Null";

    /// <inheritdoc />
    public async Task<Result<T, IError>> Run<T>(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result.Success<T, IError>(default!);
    }

    /// <inheritdoc />
    public Result<Unit, IError> Verify(StepFactoryStore stepFactoryStore)
    {
        return Unit.Default;
    }

    /// <inheritdoc />
    public bool ShouldBracketWhenSerialized => false;

    /// <inheritdoc />
    public string StepName => "Null";

    /// <summary>
    /// The Text Location where the Null constant appeared
    /// </summary>
    public TextLocation TextLocation { get; set; }

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        return this;
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<UsedVariable>, IError> GetVariablesUsed(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        return Result.Success<IReadOnlyCollection<UsedVariable>, IError>(new List<UsedVariable>());
    }

    /// <inheritdoc />
    public Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        return TypeReference.Actual.Null;
    }

    /// <inheritdoc />
    public IFreezableStep ReorganizeNamedArguments(StepFactoryStore stepFactoryStore)
    {
        return this;
    }

    /// <inheritdoc />
    public Type OutputType => typeof(SCLNull);

    /// <inheritdoc />
    string IStep.Serialize()
    {
        return "null";
    }

    /// <inheritdoc />
    public IEnumerable<Requirement> RuntimeRequirements
    {
        get
        {
            yield break;
        }
    }

    /// <inheritdoc />
    public Maybe<EntityValue> TryConvertToEntityValue()
    {
        return EntityValue.Null.Instance;
    }

    /// <summary>
    /// The value
    /// </summary>
    public object ValueObject => SCLNull.Instance;

    /// <inheritdoc />
    string IConstantFreezableStep.Serialize()
    {
        return "null";
    }

    /// <inheritdoc />
    public bool Equals(IFreezableStep? other)
    {
        return other is NullConstant;
    }

    /// <inheritdoc />
    public async Task<Result<SCLNull, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return SCLNull.Instance;
    }
}
