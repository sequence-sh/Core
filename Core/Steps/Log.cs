using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Write a value to the logs
/// </summary>
[SCLExample("Log 'Hello'", null, "Writes 'Hello' to the console.", "Hello")]
public sealed class Log : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await Value.Run<ISCLObject>(stateMonad, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Unit>();

        string stringToPrint;

        if (r.Value is StringStream ss)
            stringToPrint = ss.Serialize(SerializeOptions.Primitive);
        else
            stringToPrint = r.Value.Serialize(SerializeOptions.Serialize);

        stateMonad.Log(LogLevel.Information, stringToPrint, this);

        return Unit.Default;
    }

    /// <summary>
    /// The Value to Log.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep Value { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<Log, Unit>();

    //private sealed class LogStepFactory : GenericStepFactory
    //{
    //    private LogStepFactory() { }

    //    /// <summary>
    //    /// The instance.
    //    /// </summary>
    //    public static GenericStepFactory Instance { get; } = new LogStepFactory();

    //    /// <inheritdoc />
    //    public override Type StepType => typeof(Log);

    //    /// <inheritdoc />
    //    protected override TypeReference
    //        GetOutputTypeReference(TypeReference memberTypeReference) =>
    //        TypeReference.Unit.Instance;

    //    /// <inheritdoc />
    //    public override string OutputTypeExplanation => nameof(Unit);

    //    /// <inheritdoc />
    //    protected override Result<TypeReference, IError> GetGenericTypeParameter(
    //        CallerMetadata callerMetadata,
    //        FreezableStepData freezableStepData,
    //        TypeResolver typeResolver) => freezableStepData
    //        .TryGetStep(nameof(Log<ISCLObject>.Value), StepType)
    //        .Bind(
    //            x => x.TryGetOutputTypeReference(
    //                new CallerMetadata(
    //                    TypeName,
    //                    nameof(Log<ISCLObject>.Value),
    //                    TypeReference.Any.Instance
    //                ),
    //                typeResolver
    //            )
    //        )
    //        .Map(x => x == TypeReference.Any.Instance ? TypeReference.Actual.String : x);
    //}
}
