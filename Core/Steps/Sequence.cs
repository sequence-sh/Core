namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// A chain of steps to be run one after the other.
/// </summary>
public interface ISequenceStep
{
    /// <summary>
    /// The steps in the sequence in order
    /// </summary>
    IEnumerable<IStep> AllSteps { get; }
}

/// <summary>
/// A chain of steps to be run one after the other.
/// </summary>
public sealed class Sequence<T> : CompoundStep<T>, ISequenceStep where T : ISCLObject
{
    /// <inheritdoc />
    protected override async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        foreach (var step in InitialSteps)
        {
            var r = await step.Run(stateMonad, cancellationToken);

            if (r.IsFailure)
                return r.ConvertFailure<T>();
        }

        var finalResult = await FinalStep.Run(stateMonad, cancellationToken);

        return finalResult;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => SequenceStepFactory.Instance;

    /// <summary>
    /// The steps of this sequence apart from the final step.
    /// </summary>
    [StepListProperty]
    [DefaultValueExplanation("Empty")]
    public IReadOnlyList<IStep<Unit>> InitialSteps { get; set; } = new List<IStep<Unit>>();

    /// <summary>
    /// The final step of the sequence.
    /// Will be the return value.
    /// </summary>
    [StepProperty]
    [Required]
    public IStep<T> FinalStep { get; set; } = null!;

    /// <summary>
    /// A sequence of steps to be run one after the other.
    /// </summary>
    private sealed class SequenceStepFactory : GenericStepFactory
    {
        private SequenceStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new SequenceStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Sequence<>);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver) => freezableStepData
            .TryGetStep(nameof(Sequence<ISCLObject>.FinalStep), StepType)
            .Bind(
                x => x.TryGetOutputTypeReference(
                    new CallerMetadata(
                        TypeName,
                        nameof(Sequence<ISCLObject>.FinalStep),
                        TypeReference.Any.Instance
                    ),
                    typeResolver
                )
            );

        /// <inheritdoc />
        public override string OutputTypeExplanation => "The same type as the final step";

        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = SequenceSerializer.Instance;
    }

    /// <inheritdoc />
    public IEnumerable<IStep> AllSteps
    {
        get
        {
            foreach (var step in InitialSteps)
                yield return step;

            yield return FinalStep;
        }
    }
}
