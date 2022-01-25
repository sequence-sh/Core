namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Do an action for each element in an array or entity in an entity stream.
/// </summary>
[Alias("EntityForEach")]
[Alias("ForEachItem")]
[SCLExample("ForEach [1, 2, 3] Action: (Log <item>)",     ExpectedLogs = new[] { "1", "2", "3" })]
[SCLExample("ForEachItem In: [1, 2, 3] Do: (Log <item>)", ExpectedLogs = new[] { "1", "2", "3" })]
public sealed class ForEach<T> : CompoundStep<Unit> where T : ISCLObject
{
    /// <summary>
    /// The array or entity stream to iterate over
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("In")]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// The action to perform on each iteration
    /// </summary>
    [FunctionProperty(2)]
    [Required]
    [Alias("Do")]
    public LambdaFunction<T, Unit> Action { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var elements = await Array.Run(stateMonad, cancellationToken);

        if (elements.IsFailure)
            return elements.ConvertFailure<Unit>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async ValueTask<Result<Unit, IError>> Apply(T element, CancellationToken cancellation)
        {
            var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                Action.VariableNameOrItem,
                new KeyValuePair<VariableName, ISCLObject>(Action.VariableNameOrItem, element)
            );

            var result = await Action.StepTyped.Run(scopedMonad, cancellation);
            return result;
        }

        var finalResult = await elements.Value.ForEach(Apply, cancellationToken);

        return finalResult;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ForEachStepFactory.Instance;

    /// <summary>
    /// Do an action for each member of the list.
    /// </summary>
    private sealed class ForEachStepFactory : ArrayStepFactory
    {
        private ForEachStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new ForEachStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ForEach<>);

        /// <inheritdoc />
        public override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            TypeReference.Unit.Instance;

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return callerMetadata
                .CheckAllows(TypeReference.Unit.Instance, null)
                .Map(_ => new TypeReference.Array(TypeReference.Any.Instance) as TypeReference);
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ForEach<ISCLObject>.Array);

        /// <inheritdoc />
        protected override string LambdaPropertyName => nameof(ForEach<ISCLObject>.Action);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);
    }
}
