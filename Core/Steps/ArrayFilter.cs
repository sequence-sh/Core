namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Filter an array or entity stream using a conditional statement
/// </summary>
[Alias("FilterArray")]
[Alias("Filter")]
[SCLExample(
    "ArrayFilter [('value': 'A'), ('value': 'B'), ('value': 'C')] Predicate: (<>['value'] != 'B')",
    "[('value': \"A\"), ('value': \"C\")]"
)]
[SCLExample(
    "Filter <MyCsvFile> Using: (<>['column1'] == 'TypeA')",
    null,
    null,
    new[] { "MyCsvFile" },
    new[] { "[]" },
    ExecuteInTests = false
)]
public sealed class ArrayFilter<T> : CompoundStep<Array<T>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entityStreamResult = await Array.Run(stateMonad, cancellationToken);

        if (entityStreamResult.IsFailure)
            return entityStreamResult.ConvertFailure<Array<T>>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async IAsyncEnumerable<T> Filter(T record)
        {
            await using var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                Predicate.VariableNameOrItem,
                new KeyValuePair<VariableName, object>(Predicate.VariableNameOrItem, record!)
            );

            var result = await Predicate.StepTyped.Run(scopedMonad, cancellationToken);

            if (result.IsFailure)
                throw new ErrorException(result.Error);

            if (result.Value)
                yield return record;
        }

        var newStream = entityStreamResult.Value.SelectMany(Filter);

        return newStream;
    }

    /// <summary>
    /// The array to filter
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// A function that determines whether an entity should be included.
    /// </summary>
    [FunctionProperty(2)]
    [Required]
    [Alias("Using")]
    public LambdaFunction<T, bool> Predicate { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayFilterStepFactory.Instance;

    /// <summary>
    /// Filter entities according to a function.
    /// </summary>
    private sealed class ArrayFilterStepFactory : ArrayStepFactory
    {
        private ArrayFilterStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayFilterStepFactory();

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            new TypeReference.Array(memberTypeReference);

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return callerMetadata.ExpectedType;
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayFilter<object>.Array);

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayFilter<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";

        protected override string LambdaPropertyName => nameof(ArrayFilter<object>.Predicate);
    }
}
