namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Creates an array by repeating an element.
/// </summary>
public sealed class Repeat<T> : CompoundStep<Array<T>> where T : ISCLObject
{
    /// <summary>
    /// The element to repeat.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<T> Element { get; set; } = null!;

    /// <summary>
    /// The number of times to repeat the element
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<SCLInt> Number { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var element = await Element.Run(stateMonad, cancellationToken);

        if (element.IsFailure)
            return element.ConvertFailure<Array<T>>();

        var number = await Number.Run(stateMonad, cancellationToken);

        if (number.IsFailure)
            return number.ConvertFailure<Array<T>>();

        var result = Enumerable.Repeat(element.Value, number.Value.Value).ToSCLArray();

        return result;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => RepeatStepFactory.Instance;

    /// <summary>
    /// Creates an array by repeating an element.
    /// </summary>
    private sealed class RepeatStepFactory : GenericStepFactory
    {
        private RepeatStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new RepeatStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Repeat<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "ArrayList<T>";

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            new TypeReference.Array(memberTypeReference);

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            return freezableStepData
                .TryGetStep(nameof(Repeat<ISCLObject>.Element), StepType)
                .Bind(
                    x => x.TryGetOutputTypeReference(
                        new CallerMetadata(
                            TypeName,
                            nameof(Repeat<ISCLObject>.Element),
                            TypeReference.Any.Instance
                        ),
                        typeResolver
                    )
                );
        }
    }
}
