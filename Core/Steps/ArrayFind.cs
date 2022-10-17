namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Gets the first index of an element in an array.
/// The index starts at 0.
/// Returns -1 if the element is not present.
/// </summary>
[Alias("FindElement")]
[Alias("Find")]
[SCLExample("ArrayFind Array: [1, 2, 3] Element: 2", "1")]
[SCLExample("Find In: ['a', 'b', 'c'] Item: 'a'",    "0")]
[SCLExample("Find In: ['a', 'b', 'c'] Item: 'd'",    "-1")]
[AllowConstantFolding]
public sealed class ArrayFind<T> : CompoundStep<SCLInt> where T : ISCLObject
{
    /// <summary>
    /// The array to check.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("In")]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// The element to look for.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("Item")]
    public IStep<T> Element { get; set; } = null!;

    /// <inheritdoc />
    protected override async ValueTask<Result<SCLInt, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var arrayResult = await Array.Run(stateMonad, cancellationToken);

        if (arrayResult.IsFailure)
            return arrayResult.ConvertFailure<SCLInt>();

        var elementResult = await Element.Run(stateMonad, cancellationToken);

        if (elementResult.IsFailure)
            return elementResult.ConvertFailure<SCLInt>();

        var indexResult =
            await arrayResult.Value.IndexOfAsync(elementResult.Value, cancellationToken)
                .Map(x => x.ConvertToSCLObject());

        return indexResult;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayFindStepFactory.Instance;

    /// <summary>
    /// Gets the first index of an element in an array.
    /// </summary>
    public sealed class ArrayFindStepFactory : ArrayStepFactory
    {
        private ArrayFindStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayFindStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayFind<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Int32);

        /// <inheritdoc />
        public override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            TypeReference.Actual.Integer;

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return callerMetadata
                .CheckAllows(TypeReference.Actual.Integer, null)
                .Map(_ => new TypeReference.Array(TypeReference.Any.Instance) as TypeReference);
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayFind<ISCLObject>.Array);

        /// <inheritdoc />
        protected override string? LambdaPropertyName => null;
    }
}
