using Sequence.Core.Enums;

namespace Sequence.Core.Steps;

/// <summary>
/// Write a value to the logs
/// </summary>
[SCLExample("Log 'Hello'", null, "Logs 'Hello' using the configured provider.", "Hello")]
[SCLExample(
    "Log Message: 'Help!' Severity: Severity.Warning",
    Description = "Logs 'Help!' with severity of warning.",
    ExpectedLogs = new[] { "Help!" }
)]
public sealed class Log : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var severity = await Severity.Run(stateMonad, cancellationToken);

        if (severity.IsFailure)
            return severity.ConvertFailure<Unit>();

        var logLevel = severity.Value.Value.ConvertToLogLevel();

        var r = await Value.RunUntyped(stateMonad, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Unit>();

        string stringToPrint;

        if (r.Value is StringStream ss)
            stringToPrint = await ss.GetStringAsync();
        else
            stringToPrint = r.Value.Serialize(SerializeOptions.Serialize);

        stateMonad.Log(logLevel, stringToPrint, this);

        return Unit.Default;
    }

    /// <summary>
    /// The Value to Log.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("Message")]
    public IStep Value { get; set; } = null!;

    /// <summary>
    /// Log severity or level.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("Information")]
    [Alias("Level")]
    public IStep<SCLEnum<Severity>> Severity { get; set; } =
        new SCLConstant<SCLEnum<Severity>>(new SCLEnum<Severity>(Enums.Severity.Information));

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<Log, Unit>();
}
