using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Write a value to the logs
/// </summary>
[SCLExample("Log 'Hello'", null, "Writes 'Hello' to the console.", "Hello")]
public sealed class Log<T> : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await Value.Run(stateMonad, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Unit>();

        var stringToPrint = await SerializationMethods.GetStringAsync(r.Value);

        stateMonad.Log(LogLevel.Information, stringToPrint, this);

        return Unit.Default;
    }

    /// <summary>
    /// The Value to Log.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<T> Value { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => LogStepFactory.Instance;

    private sealed class LogStepFactory : GenericStepFactory
    {
        private LogStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new LogStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Log<>);

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
            .TryGetStep(nameof(Log<object>.Value), StepType)
            .Bind(
                x => x.TryGetOutputTypeReference(
                    new CallerMetadata(
                        TypeName,
                        nameof(Log<object>.Value),
                        TypeReference.Any.Instance
                    ),
                    typeResolver
                )
            )
            .Map(x => x == TypeReference.Any.Instance ? TypeReference.Actual.String : x);
    }
}
