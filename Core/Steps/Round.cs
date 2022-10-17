namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Round an SCL double to a fixed number of decimal places
/// </summary>
[SCLExample("Round 3.1415926535",   expectedOutput: "3.142")]
[SCLExample("Round 3.1415926535 5", expectedOutput: "3.14159")]
[SCLExample("Round 3.1415926535 0", expectedOutput: "3")]
[AllowConstantFolding]
public sealed class Round : CompoundStep<SCLDouble>
{
    /// <summary>
    /// The value to round
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("Number")]
    public IStep<SCLDouble> Value { get; set; } = null!;

    /// <summary>
    /// The number of decimal places to round to
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("3")]
    [Alias("DecimalPlaces")]
    public IStep<SCLInt> Precision { get; set; } = new SCLConstant<SCLInt>(new SCLInt(3));

    /// <inheritdoc />
    protected override async ValueTask<Result<SCLDouble, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var argsResult = await stateMonad.RunStepsAsync(Value, Precision, cancellationToken);

        if (argsResult.IsFailure)
            return argsResult.ConvertFailure<SCLDouble>();

        var (value, precision) = argsResult.Value;

        var result = Math.Round(value, precision);

        return new SCLDouble(result);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<Round, SCLDouble>();
}
