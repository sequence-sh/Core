namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A step that returns a constant value.
/// </summary>
public sealed record SCLConstant<T>(T Value) : IStep<T>, IConstantStep where T : ISCLObject
{
    /// <inheritdoc />
    #pragma warning disable CS1998
    public async ValueTask<Result<T, IError>> Run(
        #pragma warning restore CS1998
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return Value;
    }

    /// <inheritdoc />
    public ValueTask<Result<ISCLObject, IError>> RunUntyped(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) =>
        Run(stateMonad, cancellationToken).Map(x => x as ISCLObject);

    /// <inheritdoc />
    public string Name => Value.Serialize(SerializeOptions.Primitive);

    /// <inheritdoc />
    #pragma warning disable CS1998
    public async ValueTask<Result<T1, IError>> Run<T1>(
        #pragma warning restore CS1998
        IStateMonad stateMonad,
        CancellationToken cancellationToken) where T1 : ISCLObject
    {
        var r = Value.TryConvertTyped<T1>()
            .MapError(x => x.WithLocation(this));

        return r;
    }

    ISCLObject IConstantStep.Value => Value;

    /// <inheritdoc />
    public Result<Unit, IError> Verify(StepFactoryStore stepFactoryStore) => Unit.Default;

    /// <inheritdoc />
    public TextLocation? TextLocation { get; set; }

    /// <inheritdoc />
    public Type OutputType => typeof(T);

    /// <inheritdoc />
    public string Serialize(SerializeOptions options) => Value.Serialize(options);

    /// <inheritdoc />
    public void Format(
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        indentationStringBuilder.AppendPrecedingComments(remainingComments, TextLocation);
        Value.Format(indentationStringBuilder, options, remainingComments);
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
    public bool ShouldBracketWhenSerialized => false;

    /// <summary>
    /// Try to convert this constant to a constant of a different type.
    /// </summary>
    public Result<IStep, IErrorBuilder> TryConvert(Type memberType, string propertyName)
    {
        if (typeof(T) == memberType)
            return this;

        var convertedValue = Value.TryConvert(memberType);

        if (convertedValue.IsFailure)
            return convertedValue.ConvertFailure<IStep>();

        var stepType = typeof(SCLConstant<>).MakeGenericType(memberType);

        var instance = Activator.CreateInstance(stepType, convertedValue.Value);

        var instanceAsStep = (IStep)instance!;

        return Result.Success<IStep, IErrorBuilder>(instanceAsStep);
    }

    /// <inheritdoc />
    public bool HasConstantValue(IEnumerable<VariableName> providedVariables)
    {
        return true;
    }

    /// <inheritdoc />
    public ValueTask<Maybe<ISCLObject>> TryGetConstantValueAsync(
        IReadOnlyDictionary<VariableName, ISCLObject> variableValues,
        StepFactoryStore sfs) => ValueTask.FromResult(Maybe<ISCLObject>.From(Value));

    /// <inheritdoc />
    public IEnumerable<(IStep Step, IStepParameter Parameter, IStep Value)> GetParameterValues()
    {
        yield break;
    }
}
