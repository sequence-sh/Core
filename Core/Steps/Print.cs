namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Prints a value to the console.
/// </summary>
public sealed class Print<T> : CompoundStep<Unit> where T : ISCLObject
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await Value.Run(stateMonad, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Unit>();

        var stringToPrint = r.Value.Serialize(SerializeOptions.Primitive);

        stateMonad.ExternalContext.Console.WriteLine(stringToPrint);

        return Unit.Default;
    }

    /// <summary>
    /// The Value to Print.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<T> Value { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => PrintStepFactory.Instance;

    private sealed class PrintStepFactory : GenericStepFactory
    {
        private PrintStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new PrintStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Print<>);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            TypeReference.Unit.Instance;

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver) => freezableStepData
            .TryGetStep(nameof(Print<ISCLObject>.Value), StepType)
            .Bind(
                x => x.TryGetOutputTypeReference(
                    new CallerMetadata(
                        TypeName,
                        nameof(Print<ISCLObject>.Value),
                        TypeReference.Any.Instance
                    ),
                    typeResolver
                )
            )
            .Map(x => x == TypeReference.Any.Instance ? TypeReference.Actual.String : x);
    }
}
