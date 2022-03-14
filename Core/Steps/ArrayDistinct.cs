namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Removes duplicate elements in an array or entities in an entity stream.
/// By default, all entity properties are used to determine duplicates. This
/// behaviour can be changed by using the `KeySelector` parameter.
/// </summary>
[Alias("Distinct")]
[Alias("RemoveDuplicates")]
[SCLExample("[1, 2, 2, 3, 3] | RemoveDuplicates", "[1, 2, 3]", ExecuteInTests = false)]
[SCLExample(
    "[('a': 1 'b': 2), ('a': 1 'b': 2), ('a': 3 'b': 4)] | ArrayDistinct",
    "[('a': 1 'b': 2), ('a': 3 'b': 4)]"
)]
[AllowConstantFolding]
public sealed class ArrayDistinct<T> : CompoundStep<Array<T>> where T : ISCLObject
{
    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entityStreamResult = await Array.Run(stateMonad, cancellationToken);

        if (entityStreamResult.IsFailure)
            return entityStreamResult.ConvertFailure<Array<T>>();

        var ignoreCaseResult = await IgnoreCase.Run(stateMonad, cancellationToken);

        if (ignoreCaseResult.IsFailure)
            return ignoreCaseResult.ConvertFailure<Array<T>>();

        IEqualityComparer<string> comparer = ignoreCaseResult.Value.Value
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        HashSet<string> usedKeys = new(comparer);

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async IAsyncEnumerable<T> Filter(T element)
        {
            await using var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                KeySelector.VariableNameOrItem,
                new KeyValuePair<VariableName, ISCLObject>(KeySelector.VariableNameOrItem, element)
            );

            var result = await KeySelector.StepTyped.Run(scopedMonad, cancellationToken)
                .Map(async x => await x.GetStringAsync());

            if (result.IsFailure)
                throw new ErrorException(result.Error);

            if (usedKeys.Add(result.Value))
                yield return element;
        }

        var newStream = entityStreamResult.Value.SelectMany(Filter);

        return newStream;
    }

    /// <summary>
    /// The array to sort
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("In")]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// A function that gets the key to distinct by from the variable
    /// To distinct by multiple properties, concatenate several keys
    /// </summary>
    [FunctionProperty(2)]
    [DefaultValueExplanation("The item/entity")]
    [Alias("Using")]
    public LambdaFunction<T, StringStream> KeySelector { get; set; }
        = new(null, new StringInterpolate { Strings = new[] { new GetAutomaticVariable<T>() } });

    /// <summary>
    /// Whether to ignore case when comparing strings.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("False")]
    public IStep<SCLBool> IgnoreCase { get; set; } = new SCLConstant<SCLBool>(SCLBool.False);

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayDistinctStepFactory.Instance;

    /// <summary>
    /// Removes duplicate entities.
    /// </summary>
    private sealed class ArrayDistinctStepFactory : ArrayStepFactory
    {
        private ArrayDistinctStepFactory() { }

        /// <summary>
        /// The Instance
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayDistinctStepFactory();

        /// <inheritdoc />
        public override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            new TypeReference.Array(memberTypeReference);

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetada)
        {
            return callerMetada.ExpectedType;
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayDistinct<ISCLObject>.Array);

        /// <inheritdoc />
        protected override string LambdaPropertyName =>
            nameof(ArrayDistinct<ISCLObject>.KeySelector);

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayDistinct<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";
    }
}
