namespace Sequence.Core.Internal;

/// <summary>
/// A step that could have more than one possible type
/// </summary>
public abstract class OneOfStep : IStep
{
    /// <summary>
    /// The actual value of the step
    /// </summary>
    protected abstract IStep StepValue { get; }

    /// <inheritdoc />
    public ValueTask<Result<T, IError>> Run<T>(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) where T : ISCLObject
    {
        return StepValue.Run<T>(stateMonad, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<Result<ISCLObject, IError>> RunUntyped(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) => StepValue.RunUntyped(stateMonad, cancellationToken);

    /// <inheritdoc />
    public string Name => StepValue.Name;

    /// <inheritdoc />
    public Result<Unit, IError> Verify(StepFactoryStore stepFactoryStore)
    {
        return StepValue.Verify(stepFactoryStore);
    }

    /// <inheritdoc />
    public bool ShouldBracketWhenSerialized => StepValue.ShouldBracketWhenSerialized;

    /// <inheritdoc />
    public TextLocation? TextLocation
    {
        get => StepValue.TextLocation;
        set => StepValue.TextLocation = value;
    }

    /// <inheritdoc />
    public abstract Type OutputType { get; }

    /// <inheritdoc />
    public string Serialize(SerializeOptions options) => StepValue.Serialize(options);

    /// <inheritdoc />
    public void Format(
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments) => StepValue.Format(
        indentationStringBuilder,
        options,
        remainingComments
    );

    /// <inheritdoc />
    public IEnumerable<Requirement> RuntimeRequirements => StepValue.RuntimeRequirements;

    /// <inheritdoc />
    public bool HasConstantValue(IEnumerable<VariableName> providedVariables) =>
        StepValue.HasConstantValue(providedVariables);

    /// <inheritdoc />
    public ValueTask<Maybe<ISCLObject>> TryGetConstantValueAsync(
        IReadOnlyDictionary<VariableName, ISCLObject> variableValues,
        StepFactoryStore sfs) => StepValue.TryGetConstantValueAsync(variableValues, sfs);

    /// <summary>
    /// Create a OneOfStep
    /// </summary>
    public static OneOfStep Create(Type oneOfType, IStep actualStep)
    {
        var oneOfTypes = oneOfType.GenericTypeArguments;

        Type genericStepType;

        if (oneOfTypes.Length == 2)
            genericStepType = typeof(OneOfStep<,>);
        else if (oneOfTypes.Length == 3)
            genericStepType = typeof(OneOfStep<,,>);

        else
            throw new Exception($"Cannot create a OneOf with {oneOfTypes.Length} type arguments");

        var actualType = genericStepType.MakeGenericType(oneOfTypes);

        var resultStep = (OneOfStep)Activator.CreateInstance(
            actualType,
            actualStep
        )!;

        return resultStep;
    }

    /// <inheritdoc />
    public IEnumerable<(IStep Step, IStepParameter Parameter, IStep Value)> GetParameterValues()
    {
        return StepValue.GetParameterValues();
    }

    /// <inheritdoc />
    public abstract IStep FoldIfConstant(
        StepFactoryStore sfs,
        IReadOnlyDictionary<VariableName, InjectedVariable> injectedVariables);
}

/// <summary>
/// A step that could have one of two possible types
/// </summary>
public class OneOfStep<T0, T1> : OneOfStep, IStep<SCLOneOf<T0, T1>>
    where T0 : ISCLObject where T1 : ISCLObject
{
    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    private OneOfStep(OneOf<IStep<T0>, IStep<T1>> step)
    {
        Step = step;
    }

    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    public OneOfStep(IStep<T0> step0) : this(OneOf<IStep<T0>, IStep<T1>>.FromT0(step0)) { }

    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    public OneOfStep(IStep<T1> step1) : this(OneOf<IStep<T0>, IStep<T1>>.FromT1(step1)) { }

    /// <summary>
    /// The step
    /// </summary>
    public OneOf<IStep<T0>, IStep<T1>> Step { get; }

    /// <inheritdoc />
    protected override IStep StepValue => Step.Match(x => x as IStep, x => x);

    /// <inheritdoc />
    public override Type OutputType => typeof(OneOf<T0, T1>);

    /// <inheritdoc />
    public ValueTask<Result<SCLOneOf<T0, T1>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return Step.Match(
            x => x.Run(stateMonad, cancellationToken).Map(t0 => new SCLOneOf<T0, T1>(t0)),
            x => x.Run(stateMonad, cancellationToken).Map(t1 => new SCLOneOf<T0, T1>(t1))
        );
    }

    /// <inheritdoc />
    public override IStep FoldIfConstant(
        StepFactoryStore sfs,
        IReadOnlyDictionary<VariableName, InjectedVariable> injectedVariables)
    {
        var constantValueTask = TryGetConstantValueAsync(
            ImmutableDictionary<VariableName, ISCLObject>.Empty,
            sfs
        );

        if (!constantValueTask.IsCompleted)
            return this;

        var constantValue = constantValueTask.Result;

        if (constantValue.HasNoValue)
            return this;

        var t0 = constantValue.Value.MaybeAs<T0>();

        if (t0.HasValue)
            return new OneOfStep<T0, T1>(
                OneOf<IStep<T0>, IStep<T1>>.FromT0(new SCLConstant<T0>(t0.Value))
            );

        var t1 = constantValue.Value.MaybeAs<T1>();

        if (t1.HasValue)
            return new OneOfStep<T0, T1>(
                OneOf<IStep<T0>, IStep<T1>>.FromT1(new SCLConstant<T1>(t1.Value))
            );

        return this;
    }
}

/// <summary>
/// A step that could have one of two possible types
/// </summary>
public class OneOfStep<T0, T1, T2> : OneOfStep, IStep<SCLOneOf<T0, T1, T2>>
    where T0 : ISCLObject where T1 : ISCLObject where T2 : ISCLObject
{
    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    private OneOfStep(OneOf<IStep<T0>, IStep<T1>, IStep<T2>> step)
    {
        Step = step;
    }

    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    public OneOfStep(IStep<T0> step0) :
        this(OneOf<IStep<T0>, IStep<T1>, IStep<T2>>.FromT0(step0)) { }

    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    public OneOfStep(IStep<T1> step1) :
        this(OneOf<IStep<T0>, IStep<T1>, IStep<T2>>.FromT1(step1)) { }

    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    public OneOfStep(IStep<T2> step2) :
        this(OneOf<IStep<T0>, IStep<T1>, IStep<T2>>.FromT2(step2)) { }

    /// <summary>
    /// The step
    /// </summary>
    public OneOf<IStep<T0>, IStep<T1>, IStep<T2>> Step { get; }

    /// <inheritdoc />
    protected override IStep StepValue => Step.Match(x => x as IStep, x => x, x => x);

    /// <inheritdoc />
    public override Type OutputType => typeof(OneOf<T0, T1, T2>);

    /// <inheritdoc />
    public ValueTask<Result<SCLOneOf<T0, T1, T2>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return Step.Match(
            x => x.Run(stateMonad, cancellationToken).Map(t0 => new SCLOneOf<T0, T1, T2>(t0)),
            x => x.Run(stateMonad, cancellationToken).Map(t1 => new SCLOneOf<T0, T1, T2>(t1)),
            x => x.Run(stateMonad, cancellationToken).Map(t2 => new SCLOneOf<T0, T1, T2>(t2))
        );
    }

    /// <inheritdoc />
    public override IStep FoldIfConstant(
        StepFactoryStore sfs,
        IReadOnlyDictionary<VariableName, InjectedVariable> injectedVariables)
    {
        var constantValueTask = TryGetConstantValueAsync(
            ImmutableDictionary<VariableName, ISCLObject>.Empty,
            sfs
        );

        if (!constantValueTask.IsCompleted)
            return this;

        var constantValue = constantValueTask.Result;

        if (constantValue.HasNoValue)
            return this;

        var t0 = constantValue.Value.MaybeAs<T0>();

        if (t0.HasValue)
            return new OneOfStep<T0, T1, T2>(
                OneOf<IStep<T0>, IStep<T1>, IStep<T2>>.FromT0(new SCLConstant<T0>(t0.Value))
            );

        var t1 = constantValue.Value.MaybeAs<T1>();

        if (t1.HasValue)
            return new OneOfStep<T0, T1, T2>(
                OneOf<IStep<T0>, IStep<T1>, IStep<T2>>.FromT1(new SCLConstant<T1>(t1.Value))
            );

        var t2 = constantValue.Value.MaybeAs<T2>();

        if (t2.HasValue)
            return new OneOfStep<T0, T1, T2>(
                OneOf<IStep<T0>, IStep<T1>, IStep<T2>>.FromT2(new SCLConstant<T2>(t2.Value))
            );

        return this;
    }
}
