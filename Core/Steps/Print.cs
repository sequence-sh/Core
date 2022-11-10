namespace Sequence.Core.Steps;

/// <summary>
/// Prints a value to the console.
/// </summary>
[Alias("ConsoleWrite")]
[Alias("WriteToConsole")]
public sealed class Print : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
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

        stateMonad.ExternalContext.Console.WriteLine(stringToPrint);

        return Unit.Default;
    }

    /// <summary>
    /// The Value to Print.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("Message")]
    public IStep Value { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<Print, Unit>();
}
