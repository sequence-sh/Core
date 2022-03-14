namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Gets the first element of an array or entity stream
/// </summary>
[Alias("First")]
[Alias("GetFirstItem")]
[SCLExample("ArrayFirst [1,2,3]",                        ExpectedOutput = "1")]
[SCLExample("ArrayFirst ['a', 'b', 'c']",                ExpectedOutput = "a")]
[SCLExample("ArrayFirst [('a': 1), ('a': 2), ('a': 3)]", ExpectedOutput = "('a': 1)")]
[SCLExample("GetFirstItem In: [1,2,3]",                  ExpectedOutput = "1")]
[AllowConstantFolding]
public sealed class ArrayFirst<T> : CompoundStep<T> where T : ISCLObject
{
    /// <inheritdoc />
    protected override Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return Array.Run(stateMonad, cancellationToken)
            .Bind(x => x.ElementAtAsync(0, new ErrorLocation(this), cancellationToken));
    }

    /// <summary>
    /// The array to get the first element of
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("In")]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayFirstStepFactory.Instance;

    /// <summary>
    /// Gets the array element at a particular index.
    /// </summary>
    private sealed class ArrayFirstStepFactory : ArrayStepFactory
    {
        private ArrayFirstStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayFirstStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayFirst<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        public override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return new TypeReference.Array(callerMetadata.ExpectedType);
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayFirst<ISCLObject>.Array);

        /// <inheritdoc />
        protected override string? LambdaPropertyName => null;
    }
}
