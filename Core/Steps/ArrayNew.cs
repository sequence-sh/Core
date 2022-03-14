namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// A step that declares a new array
/// </summary>
public interface IArrayNewStep
{
    /// <summary>
    /// The elements of the array
    /// </summary>
    IEnumerable<IStep> ElementSteps { get; } //This is used by ChainInfixSerializer
}

/// <summary>
/// Represents an ordered collection of objects.
/// </summary>
[Alias("Array")]
[Alias("NewArray")]
[Alias("ArrayCreate")]
[AllowConstantFolding]
public sealed class ArrayNew<T> : CompoundStep<Array<T>>, IArrayNewStep where T : ISCLObject
{
    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await Elements.Select(x => x.Run(stateMonad, cancellationToken))
            .Combine(ErrorList.Combine)
            .Map(x => x.ToList().ToSCLArray());

        return result;
    }

    /// <inheritdoc />
    public override bool ShouldBracketWhenSerialized => false;

    /// <summary>
    /// The elements of the array.
    /// </summary>
    [StepListProperty(1)]
    [Required]
    [Alias("Items")]
    public IReadOnlyList<IStep<T>> Elements { get; set; } = null!;

    /// <inheritdoc />
    public IEnumerable<IStep> ElementSteps => Elements;

    /// <summary>
    /// Creates an array.
    /// </summary>
    public static ArrayNew<T> CreateArray(List<IStep<T>> stepList)
    {
        return new() { Elements = stepList };
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayNewStepFactory.Instance;

    /// <summary>
    /// The factory for creating Arrays.
    /// </summary>
    private class ArrayNewStepFactory : GenericStepFactory
    {
        private ArrayNewStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayNewStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayNew<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";

        /// <inheritdoc />
        public override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            new TypeReference.Array(memberTypeReference);

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var mtr = callerMetadata.ExpectedType.TryGetArrayMemberTypeReference(typeResolver)
                .MapError(x => x.WithLocation(freezableStepData));

            if (mtr.IsFailure)
                return mtr.ConvertFailure<TypeReference>();

            var result =
                freezableStepData.TryGetStepList(nameof(ArrayNew<ISCLObject>.Elements), StepType)
                    .Bind(
                        x => x.Select(
                                r => r.TryGetOutputTypeReference(
                                    new CallerMetadata(
                                        TypeName,
                                        nameof(ArrayNew<SCLInt>.Elements),
                                        mtr.Value
                                    ),
                                    typeResolver
                                )
                            )
                            .Combine(ErrorList.Combine)
                    )
                    .Map(TypeReference.Create);

            return result;
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer => ArraySerializer.Instance;
    }
}
