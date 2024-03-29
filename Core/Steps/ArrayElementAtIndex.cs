﻿namespace Sequence.Core.Steps;

/// <summary>
/// Gets the array element at a particular index.
/// The first element has an index of 0.
/// </summary>
[Alias("FromArray")]
[Alias("ElementAtIndex")]
[SCLExample("FromArray ['A', 'B', 'C'] GetElement: 1", "B")]
[AllowConstantFolding]
public sealed class ArrayElementAtIndex<T> : CompoundStep<T> where T : ISCLObject
{
    /// <summary>
    /// The array to check.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// The index to get the element at.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("GetElement")]
    public IStep<SCLInt> Index { get; set; } = null!;

    /// <inheritdoc />
    protected override async ValueTask<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(Array, Index, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<T>();

        var (array, index) = r.Value;

        var result = await array.ElementAtAsync(
            index.Value,
            new ErrorLocation(this),
            cancellationToken
        );

        return result;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayElementAtIndexStepFactory.Instance;

    /// <inheritdoc />
    public override bool ShouldBracketWhenSerialized => false;

    /// <summary>
    /// Gets the array element at a particular index.
    /// </summary>
    private sealed class ArrayElementAtIndexStepFactory : ArrayStepFactory
    {
        private ArrayElementAtIndexStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayElementAtIndexStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayElementAtIndex<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        public override IStepSerializer Serializer => ArrayElementAtIndexSerializer.Instance;

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
        protected override string ArrayPropertyName =>
            nameof(ArrayElementAtIndex<ISCLObject>.Array);

        /// <inheritdoc />
        protected override string? LambdaPropertyName => null;
    }
}
