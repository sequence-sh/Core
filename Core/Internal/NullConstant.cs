namespace Reductech.Sequence.Core.Internal;

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
        CancellationToken cancellationToken) where T : ISCLObject
    {
        await Task.CompletedTask;

        var r = (SCLNull.Instance as ISCLObject).TryConvertTyped<T>("Step")
            .MapError(x => x.WithLocation(TextLocation ?? ErrorLocation.EmptyLocation));

        return r;
    }

    /// <inheritdoc />
    public Task<Result<ISCLObject, IError>> RunUntyped(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) =>
        Run(stateMonad, cancellationToken).Map(x => x as ISCLObject);

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
        return CheckFreezePossible(callerMetadata, typeResolver).Map(() => this as IStep);
    }

    /// <inheritdoc />
    public UnitResult<IError> CheckFreezePossible(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        if (callerMetadata.ExpectedType.Allow(TypeReference.Actual.Null, typeResolver))
            return UnitResult.Success<IError>();

        return ErrorCode.InvalidCast.ToErrorBuilder(callerMetadata.ParameterName, Value)
            .WithLocationSingle(TextLocation);
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
    public string Serialize(SerializeOptions options) => SCLNull.Instance.Serialize(options);

    /// <inheritdoc />
    public void Format(
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        indentationStringBuilder.AppendPrecedingComments(remainingComments, TextLocation);

        indentationStringBuilder.Append($"{Serialize(SerializeOptions.Serialize)}");
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
    public bool Equals(IFreezableStep? other)
    {
        return other is NullConstant;
    }

    /// <inheritdoc />
    public async Task<Result<SCLNull, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return SCLNull.Instance;
    }

    /// <summary>
    /// SCLNull
    /// </summary>
    public ISCLObject Value => SCLNull.Instance;

    /// <inheritdoc />
    public Result<IStep, IErrorBuilder> TryConvert(Type memberType, string propertyName)
    {
        return ErrorCode.InvalidCast.ToErrorBuilder(propertyName, Name);
    }

    /// <inheritdoc />
    public bool HasConstantValue(IEnumerable<VariableName> providedVariables) => true;

    /// <inheritdoc />
    public ValueTask<Maybe<ISCLObject>> TryGetConstantValueAsync(
        IReadOnlyDictionary<VariableName, ISCLObject> variableValues,
        StepFactoryStore sfs) => ValueTask.FromResult(Maybe<ISCLObject>.From(SCLNull.Instance));

    /// <inheritdoc />
    public IEnumerable<(IStep Step, IStepParameter Parameter, IStep Value)> GetParameterValues()
    {
        yield break;
    }
}
