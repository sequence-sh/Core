using Microsoft.Extensions.Logging;

namespace Reductech.Sequence.Core.Steps;

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
        var r = await Value.RunUntyped(stateMonad, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Unit>();

        string stringToPrint;

        if (r.Value is StringStream ss)
            stringToPrint = await ss.GetStringAsync();
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
}
