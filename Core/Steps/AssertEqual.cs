namespace Sequence.Core.Steps;

/// <summary>
/// Asserts that two objects are equal.
/// Both objects must have the same type.
/// </summary>
public sealed class AssertEqual<T> : CompoundStep<Unit> where T : ISCLObject
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var left = await Left.Run(stateMonad, cancellationToken);

        if (left.IsFailure)
            return left.ConvertFailure<Unit>();

        var right = await Right.Run(stateMonad, cancellationToken);

        if (right.IsFailure)
            return right.ConvertFailure<Unit>();

        if (left.Value.Equals(right.Value))
            return Unit.Default;

        var lString = left.Value.Serialize(SerializeOptions.Primitive);
        var rString = right.Value.Serialize(SerializeOptions.Primitive);

        var error = ErrorCode.AssertionFailed
            .ToErrorBuilder($"Expected {lString} to equal {rString}")
            .WithLocation(this);

        return Result.Failure<Unit, IError>(error);
    }

    /// <summary>
    /// The first object to compare
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<T> Left { get; set; } = null!;

    /// <summary>
    /// The second object to compare
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<T> Right { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = AssertEqualStepFactory.Instance;

    private sealed class AssertEqualStepFactory : GenericStepFactory
    {
        private AssertEqualStepFactory() { }
        public static GenericStepFactory Instance { get; } = new AssertEqualStepFactory();

        /// <inheritdoc />
        public override Type StepType { get; } = typeof(AssertEqual<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation { get; } = nameof(Unit);

        /// <inheritdoc />
        public override TypeReference GetOutputTypeReference(TypeReference memberTypeReference)
        {
            return TypeReference.Unit.Instance;
        }

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var left = freezableStepData
                .TryGetStep(nameof(Left), StepType)
                .Bind(
                    x => x.TryGetOutputTypeReference(
                        new CallerMetadata(TypeName, nameof(Left), TypeReference.Any.Instance),
                        typeResolver
                    )
                );

            if (left.IsFailure)
                return left.ConvertFailure<TypeReference>();

            var right = freezableStepData
                .TryGetStep(nameof(Right), StepType)
                .Bind(
                    x => x.TryGetOutputTypeReference(
                        new CallerMetadata(TypeName, nameof(Right), TypeReference.Any.Instance),
                        typeResolver
                    )
                );

            if (right.IsFailure)
                return right.ConvertFailure<TypeReference>();

            var r = TypeReference.Create(new[] { left.Value, right.Value });

            return r;
        }
    }
}
