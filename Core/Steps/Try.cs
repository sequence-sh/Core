using Reductech.Sequence.Core.Internal.Logging;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Tries to execute a step and recovers if that step results in failure.
/// </summary>
[Alias("TryTo")]
[SCLExample(
    "Try (1 / 0) OnError: 0",
    "0",
    ExpectedLogs = new[] { "Error Caught in Divide: Attempt to Divide by Zero." }
)]
[SCLExample("Try (4 / 2)", "2")]
[SCLExample(
    "Try (1 / 0)",
    "0",
    Description = "If the alternative is not set the default value is used.",
    ExpectedLogs = new[] { "Error Caught in Divide: Attempt to Divide by Zero." }
)]
[SCLExample(
    "Try (ArrayElementAtIndex [0,1,2,3] 4 ) OnError: 4",
    "4",
    ExpectedLogs = new[]
    {
        "Error Caught in ArrayElementAtIndex: Index was outside the bounds of the array."
    }
)]
[SCLExample(
    @"TryTo Do: (
- log 123
- log 1 / 0
- 4
) OnError: 5",
    "5",
    ExpectedLogs = new[] { "123", "Error Caught in Sequence: Attempt to Divide by Zero." }
)]
public sealed class Try<T> : CompoundStep<T> where T : ISCLObject
{
    /// <inheritdoc />
    protected override async ValueTask<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var statementResult = await Statement.Run(stateMonad, cancellationToken);

        if (statementResult.IsSuccess)
            return statementResult.Value;

        var message = statementResult.Error.AsString;

        LogSituation.StepErrorWasCaught.Log(
            stateMonad,
            this,
            Statement.Name,
            message
        );

        if (Recover is null)
            return DefaultValues.GetDefault<T>();

        var scopedStateMonad = new ScopedStateMonad(
            stateMonad,
            stateMonad.GetState().ToImmutableDictionary(),
            VariableName.Item,
            new KeyValuePair<VariableName, ISCLObject>(
                Recover.VariableNameOrItem,
                new StringStream(message)
            )
        );

        var result = await Recover.Step.Run<T>(scopedStateMonad, cancellationToken);

        return result;
    }

    /// <summary>
    /// The statement to try.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("Sequence")]
    [Alias("Do")]
    public IStep<T> Statement { get; set; } = null!;

    /// <summary>
    /// The action to perform on an error.
    /// </summary>
    [FunctionProperty()]
    [Alias("OnError")]
    [DefaultValueExplanation("Returns the default value of the return type.")]
    public LambdaFunction<StringStream, T>? Recover { get; set; } = null;

    /// <inheritdoc />
    public override IStepFactory StepFactory => TryStepFactory.Instance;

    private sealed class TryStepFactory : GenericStepFactory
    {
        private TryStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new TryStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Try<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        public override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var statementType = freezableStepData
                .TryGetStep(nameof(Statement), StepType)
                .Bind(
                    x => x.TryGetOutputTypeReference(
                        new CallerMetadata(TypeName, nameof(Statement), TypeReference.Any.Instance),
                        typeResolver
                    )
                );

            if (statementType.IsFailure)
                return statementType.ConvertFailure<TypeReference>();

            var alternativeStep = freezableStepData.TryGetStep(nameof(Recover), StepType);

            if (alternativeStep.IsFailure)
                return statementType.Value;

            var alternativeType = alternativeStep.Value.TryGetOutputTypeReference(
                new CallerMetadata(TypeName, nameof(Recover), TypeReference.Any.Instance),
                typeResolver
            );

            if (alternativeType.IsFailure)
                return alternativeType.ConvertFailure<TypeReference>();

            var r = TypeReference.Create(new[] { statementType.Value, alternativeType.Value });
            return r;
        }
    }
}
