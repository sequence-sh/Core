namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Checks if an array is empty.
/// </summary>
[Alias("IsArrayEmpty")]
[Alias("IsEmpty")]
public sealed class ArrayIsEmpty<T> : CompoundStep<SCLBool> where T : ISCLObject
{
    /// <summary>
    /// The array to check for emptiness.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<SCLBool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return await Array.Run(stateMonad, cancellationToken)
            .Bind(x => x.AnyAsync(cancellationToken))
            .Map(x => x.ConvertToSCLObject());
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayIsEmptyStepFactory.Instance;

    /// <summary>
    /// Checks if an array is empty.
    /// </summary>
    private sealed class ArrayIsEmptyStepFactory : ArrayStepFactory
    {
        private ArrayIsEmptyStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayIsEmptyStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayIsEmpty<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Boolean);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => TypeReference.Actual.Bool;

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayIsEmpty<ISCLObject>.Array);

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return callerMetadata
                .CheckAllows(TypeReference.Actual.Bool, null)
                .Map(_ => new TypeReference.Array(TypeReference.Any.Instance) as TypeReference);
        }

        /// <inheritdoc />
        protected override string? LambdaPropertyName => null;
    }
}
