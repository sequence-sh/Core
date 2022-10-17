using System.Globalization;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Converts a date to the specified format, yyyyMMddHHmm by default.
/// If no date is specified, returns the current time.
/// </summary>
[Alias("ConvertStringToDate")]
[Alias("ToDate")]
[SCLExample("StringToDate '2020/10/20 20:30:40'", "2020-10-20T20:30:40.0000000")]
[AllowConstantFolding]
public sealed class StringToDate : CompoundStep<SCLDateTime>
{
    /// <summary>
    /// The string to convert to DateTime
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Example("2020/11/22 20:55:11")]
    [Alias("String")]
    public IStep<StringStream> Date { get; set; } = null!;

    /// <summary>
    /// The input format to use for conversion.
    /// If not set, will use DateTime.Parse by default.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("Will use DateTime to try and convert.")]
    [Example("yyyy/MM/dd HH:mm:ss")]
    [Alias("Format")]
    public IStep<StringStream>? InputFormat { get; set; } = null;

    /// <summary>
    /// The culture to use for date conversion. Default is current culture.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("Current culture")]
    [Example("en-GB")]
    public IStep<StringStream> Culture { get; set; } =
        new SCLConstant<StringStream>(CultureInfo.CurrentCulture.Name);

    /// <inheritdoc />
    protected override async ValueTask<Result<SCLDateTime, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var dateResult = await Date.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (dateResult.IsFailure)
            return dateResult.ConvertFailure<SCLDateTime>();

        string? inputFormat = null;

        if (InputFormat != null)
        {
            var inputFormatResult = await InputFormat.Run(stateMonad, cancellationToken)
                .Map(async x => await x.GetStringAsync());

            if (inputFormatResult.IsFailure)
                return inputFormatResult.ConvertFailure<SCLDateTime>();

            inputFormat = inputFormatResult.Value;
        }

        var cultureResult = await Culture.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (cultureResult.IsFailure)
            return cultureResult.ConvertFailure<SCLDateTime>();

        CultureInfo ci;

        try
        {
            ci = new CultureInfo(cultureResult.Value);
        }
        catch (CultureNotFoundException)
        {
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.CouldNotParse,
                cultureResult.Value,
                nameof(Culture)
            );
        }

        DateTime date;

        if (inputFormat == null)
        {
            try
            {
                date = DateTime.Parse(dateResult.Value, ci);
            }
            catch (FormatException fe)
            {
                return new SingleError(
                    new ErrorLocation(this),
                    fe,
                    ErrorCode.CouldNotParse
                );
            }
        }
        else
        {
            try
            {
                date = DateTime.ParseExact(dateResult.Value, inputFormat, ci);
            }
            catch (FormatException fe)
            {
                return new SingleError(
                    new ErrorLocation(this),
                    fe,
                    ErrorCode.CouldNotParse
                );
            }
        }

        return date.ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringToDate, SCLDateTime>();
}
