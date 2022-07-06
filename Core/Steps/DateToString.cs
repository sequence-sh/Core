namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Converts a date to the specified format, yyyy/MM/dd HH:mm:ss by default.
/// If no date is specified, uses the current date and time.
/// </summary>
[Alias("DateNow")]
[Alias("ConvertDateToString")]
[Alias("FormatDate")]
[SCLExample("DateToString 1990-01-06T09:15:00",              "1990/01/06 09:15:00")]
[SCLExample("DateToString 1990-01-06T09:15:00 'yyyy/MM/dd'", "1990/01/06")]
[SCLExample("FormatDate 2022-01-01T01:01:01 As: 'HH:mm:ss'", "01:01:01")]
[AllowConstantFolding]
public sealed class DateToString : CompoundStep<StringStream>
{
    /// <summary>
    /// The date and time to convert to the specified format.
    /// </summary>
    [StepProperty(1)]
    [DefaultValueExplanation("DateTime.Now")]
    public IStep<SCLDateTime> Date { get; set; } =
        new SCLConstant<SCLDateTime>(DateTime.Now.ConvertToSCLObject());

    /// <summary>
    /// The output format to use for the date.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("yyyy/MM/dd HH:mm:ss")]
    [Example("O")]
    [Alias("As")]
    public new IStep<StringStream> Format { get; set; } =
        new SCLConstant<StringStream>("yyyy/MM/dd HH:mm:ss");

    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(Date, Format.WrapStringStream(), cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<StringStream>();

        var (date, format) = r.Value;

        var dateOut = date.Value.ToString(format);

        return new StringStream(dateOut);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<DateToString, StringStream>();
}
