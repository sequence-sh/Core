namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// The direction to sort the elements
/// </summary>
public enum SortOrder
{
    /// <summary>
    /// Ascending order. 'a' first
    /// </summary>
    Ascending,

    /// <summary>
    /// Descending order. 'z' first
    /// </summary>
    Descending
}

/// <summary>
/// Reorder an array or entity stream
/// </summary>
[Alias("SortArray")]
[Alias("Sort")]
[SCLExample("ArraySort [2, 4, 1, 3] Descending: true", "[4, 3, 2, 1]")]
[SCLExample(
    @"Sort [
  ('type': 'C', 'value': 1)
  ('type': 'A', 'value': 2)
  ('type': 'B', 'value': 3)
] Using: (<>['type'])",
    "[('type': \"A\" 'value': 2), ('type': \"B\" 'value': 3), ('type': \"C\" 'value': 1)]"
)]
[AllowConstantFolding]
public sealed class ArraySort<T> : CompoundStep<Array<T>> where T : ISCLObject
{
    /// <summary>
    /// The array or entity stream to sort
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// A function that gets the key to sort by. Use this function to select
    /// an entity property that will be used for sorting.
    /// To sort by multiple properties, concatenate several keys
    /// </summary>
    [FunctionProperty(2)]
    [DefaultValueExplanation("Default Ordering")]
    [Alias("Using")]
    public LambdaFunction<T, StringStream>? KeySelector { get; set; } = null!;

    /// <summary>
    /// Whether to sort in descending order.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("False")]
    public IStep<SCLOneOf<SCLBool, SCLEnum<SortOrder>>> Descending { get; set; } =
        new OneOfStep<SCLBool, SCLEnum<SortOrder>>(new SCLConstant<SCLBool>(SCLBool.False));

    /// <inheritdoc />
    protected override async ValueTask<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var array = await Array.Run(stateMonad, cancellationToken);

        if (array.IsFailure)
            return array.ConvertFailure<Array<T>>();

        var descending = await Descending.Run(stateMonad, cancellationToken);

        if (descending.IsFailure)
            return descending.ConvertFailure<Array<T>>();

        var sortOrderIsDescending = descending.Value.OneOf.Match(
            x => x.Value,
            x => x.Value == SortOrder.Descending
        );

        Array<T> sortedArray;

        if (KeySelector == null)
        {
            sortedArray = array.Value.Sort(sortOrderIsDescending);
        }
        else
        {
            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async ValueTask<string> GetKey(T entity, CancellationToken cancellation)
            {
                var scopedMonad = new ScopedStateMonad(
                    stateMonad,
                    currentState,
                    KeySelector.VariableNameOrItem,
                    new KeyValuePair<VariableName, ISCLObject>(
                        KeySelector.VariableNameOrItem,
                        entity
                    )
                );

                var result = await KeySelector.StepTyped.Run(scopedMonad, cancellation)
                    .Map(x => x.GetStringAsync());

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                return result.Value;
            }

            sortedArray = array.Value.Sort(sortOrderIsDescending, GetKey);
        }

        return sortedArray;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArraySortStepFactory.Instance;

    /// <summary>
    /// Reorder an array.
    /// </summary>
    private sealed class ArraySortStepFactory : ArrayStepFactory
    {
        private ArraySortStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArraySortStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArraySort<>);

        /// <inheritdoc />
        public override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            new TypeReference.Array(memberTypeReference);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return callerMetadata.ExpectedType;
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArraySort<ISCLObject>.Array);

        protected override string LambdaPropertyName => nameof(ArraySort<ISCLObject>.KeySelector);
    }
}
