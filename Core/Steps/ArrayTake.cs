namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Take the first n elements of an array or entity stream
/// </summary>
[Alias("Take")]
[SCLExample("ArrayTake [1, 2, 3] 2",               "[1, 2]")]
[SCLExample("Take From: [1, 2, 3, 4, 5] Count: 3", "[1, 2, 3]")]
public sealed class ArrayTake<T> : CompoundStep<Array<T>> where T : ISCLObject
{
    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(Array, Count, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Array<T>>();

        var (array, count) = r.Value;

        return array.Take(count.Value);
    }

    /// <summary>
    /// The array or entity stream to take elements from
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("From")]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// The number of elements/entities to take
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<SCLInt> Count { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = ArrayTakeStepFactory.Instance;

    /// <summary>
    /// Counts the elements in an array.
    /// </summary>
    private sealed class ArrayTakeStepFactory : ArrayStepFactory
    {
        private ArrayTakeStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayTakeStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayTake<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";

        /// <inheritdoc />
        public override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            new TypeReference.Array(memberTypeReference);

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return callerMetadata.ExpectedType;
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayTake<ISCLObject>.Array);

        protected override string? LambdaPropertyName => null;
    }
}
