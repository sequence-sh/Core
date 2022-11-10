namespace Sequence.Core.Steps;

/// <summary>
/// Reverse the order of elements in an array
/// </summary>
[SCLExample("ArrayReverse [1, 2, 3]", "[3, 2, 1]")]
[AllowConstantFolding]
public sealed class ArrayReverse<T> : CompoundStep<Array<T>> where T : ISCLObject
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var arrayResult = await Array.Run(stateMonad, cancellationToken)
            .Bind(x => x.Evaluate(cancellationToken));

        if (arrayResult.IsFailure)
            return arrayResult.ConvertFailure<Array<T>>();

        return arrayResult.Value.List.Reverse().ToSCLArray();
    }

    /// <summary>
    /// The array to reverse
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayReverseStepFactory.Instance;

    /// <summary>
    /// Removes duplicate entities.
    /// </summary>
    private sealed class ArrayReverseStepFactory : ArrayStepFactory
    {
        private ArrayReverseStepFactory() { }

        /// <summary>
        /// The Instance
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayReverseStepFactory();

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
        protected override string ArrayPropertyName => nameof(ArrayReverse<ISCLObject>.Array);

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayReverse<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";

        /// <inheritdoc />
        protected override string? LambdaPropertyName => null;
    }
}
