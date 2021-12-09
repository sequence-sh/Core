namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Concatenates arrays or streams of entities
/// </summary>
[SCLExample("[1,2,3] + [4,5,6]",                      "[1, 2, 3, 4, 5, 6]")]
[SCLExample("ArrayConcat [[1,2,3], [4,5,6]]",         "[1, 2, 3, 4, 5, 6]")]
[SCLExample("Combine Arrays: [[1, 2, 3], [4, 5, 6]]", "[1, 2, 3, 4, 5, 6]")]
[Alias("Combine")]
public sealed class ArrayConcat<T> : CompoundStep<Array<T>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var streamsResult = await Arrays.Run(stateMonad, cancellationToken);

        if (streamsResult.IsFailure)
            return streamsResult.ConvertFailure<Array<T>>();

        var result =
            streamsResult.Value.SelectMany(al => al.GetAsyncEnumerable());

        return result;
    }

    /// <summary>
    /// The arrays to concatenate
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Array<T>>> Arrays { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayConcatStepFactory.Instance;

    /// <summary>
    /// Concatenates streams of entities
    /// </summary>
    private sealed class ArrayConcatStepFactory : GenericStepFactory
    {
        private ArrayConcatStepFactory() { }

        /// <summary>
        /// The Instance
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayConcatStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayConcat<>);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            new TypeReference.Array(memberTypeReference);

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetada,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var expectedMemberType = callerMetada.ExpectedType
                .TryGetArrayMemberTypeReference(typeResolver)
                .MapError(x => x.WithLocation(freezableStepData));

            if (expectedMemberType.IsFailure)
                return expectedMemberType.ConvertFailure<TypeReference>();

            var arraysStep = freezableStepData.TryGetStep(
                nameof(ArrayConcat<object>.Arrays),
                StepType
            );

            if (arraysStep.IsFailure)
                return arraysStep.ConvertFailure<TypeReference>();

            var expectedArraysStepType =
                new TypeReference.Array(new TypeReference.Array(expectedMemberType.Value));

            var arraysStepGenericType = arraysStep.Value.TryGetOutputTypeReference(
                        new CallerMetadata(
                            TypeName,
                            nameof(ArrayConcat<object>.Arrays),
                            expectedArraysStepType
                        ),
                        typeResolver
                    )
                    .Bind(
                        x => x.TryGetArrayMemberTypeReference(typeResolver)
                            .MapError(e => e.WithLocation(freezableStepData))
                    )
                    .Bind(
                        x => x.TryGetArrayMemberTypeReference(typeResolver)
                            .MapError(e => e.WithLocation(freezableStepData))
                    )
                ;

            if (arraysStepGenericType.IsFailure)
                return arraysStepGenericType.ConvertFailure<TypeReference>();

            return arraysStepGenericType;
        }

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";
    }
}
