namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Skip the first n elements of an array or entity stream
/// </summary>
[Alias("Skip")]
[SCLExample("ArraySkip [1, 2, 3] 2",             "[3]")]
[SCLExample("Skip In: [1, 2, 3, 4, 5] Count: 3", "[4, 5]")]
[AllowConstantFolding]
public sealed class ArraySkip<T> : CompoundStep<Array<T>> where T : ISCLObject
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

        return array.Skip(count.Value);
    }

    /// <summary>
    /// The array to skip elements from
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("In")]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// The number of elements/entities to skip
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<SCLInt> Count { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = ArraySkipStepFactory.Instance;

    /// <summary>
    /// Counts the elements in an array.
    /// </summary>
    private sealed class ArraySkipStepFactory : ArrayStepFactory
    {
        private ArraySkipStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArraySkipStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArraySkip<>);

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
        protected override string ArrayPropertyName => nameof(ArraySkip<ISCLObject>.Array);

        protected override string? LambdaPropertyName => null;
    }
}
