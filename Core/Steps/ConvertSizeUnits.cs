using System.Text.RegularExpressions;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Convert a string representing a data size to  a string representing the same size but with different units.
/// </summary>
[SCLExample(
    "ConvertSizeUnits '1024 Kb' SizeUnit.Megabytes",
    "1.00 MB",
    description: "Basic Replacement"
)]
[SCLExample(
    "ConvertSizeUnits '1024' SizeUnit.Kilobytes",
    "1.00 KB",
    description: "Basic Replacement"
)]
[SCLExample(
    "ConvertSizeUnits '10 Kb' SizeUnit.Bytes",
    "10240.00 B",
    description: "Basic Replacement"
)]
[SCLExample(
    "ConvertSizeUnits '1.5 Kb' SizeUnit.Bytes",
    "1536.00 B",
    description: "Basic Replacement"
)]
[SCLExample(
    "ConvertSizeUnits '250 Kb' SizeUnit.Megabytes",
    "0.24 MB",
    description: "Basic Replacement"
)]
[AllowConstantFolding]
public sealed class ConvertSizeUnits : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(String.WrapStringStream(), Units, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<StringStream>();

        var (text, unit) = r.Value;

        var match = ValueRegex.Match(text);

        if (!match.Success)
            return ErrorCode.CouldNotParse.ToErrorBuilder(text, "data size")
                .WithLocationSingle(this);

        var originalNumber = double.Parse(match.Groups["num"].Value);

        double multiplier;

        if (match.Groups["units"].Success)
        {
            multiplier = match.Groups["units"].Value.ToLowerInvariant() switch
            {
                "b"  => BytesMultiplier,
                "kb" => KBytesMultiplier,
                "mb" => MBytesMultiplier,
                "gb" => GBytesMultiplier,
                "tb" => TBytesMultiplier,
                "pb" => PBytesMultiplier,
                "eb" => EBytesMultiplier,
                _    => throw new ArgumentOutOfRangeException(nameof(unit))
            };
        }
        else
        {
            multiplier = 1;
        }

        var desiredMultiplier = unit.Value switch
        {
            SizeUnit.Bytes     => BytesMultiplier,
            SizeUnit.Kilobytes => KBytesMultiplier,
            SizeUnit.Megabytes => MBytesMultiplier,
            SizeUnit.Gigabytes => GBytesMultiplier,
            SizeUnit.Terabytes => TBytesMultiplier,
            SizeUnit.Petabyte  => PBytesMultiplier,
            SizeUnit.Exabyte   => EBytesMultiplier,
            _                  => throw new ArgumentOutOfRangeException(nameof(unit))
        };

        var ratio = multiplier / desiredMultiplier;

        var finalNumber = ratio * originalNumber;

        var finalUnits = unit.Value switch
        {
            SizeUnit.Bytes     => "B",
            SizeUnit.Kilobytes => "KB",
            SizeUnit.Megabytes => "MB",
            SizeUnit.Gigabytes => "GB",
            SizeUnit.Terabytes => "TB",
            SizeUnit.Petabyte  => "PB",
            SizeUnit.Exabyte   => "EB",
            _                  => throw new ArgumentOutOfRangeException(nameof(unit))
        };

        var finalText = $"{finalNumber:F2} {finalUnits}";

        return new StringStream(finalText);

        //var v = originalNumber * multiplier;
    }

    private static readonly Regex ValueRegex = new(
        @"\A\s*(?<num>\d+(?:\.\d+)?)\s*(?<units>b|kb|mb|gb|tb|pb|eb)?\Z",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <summary>
    /// The string containing the original value and units
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The units to convert to
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("To")]
    public IStep<SCLEnum<SizeUnit>> Units { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<ConvertSizeUnits, StringStream>();

    private const double BytesMultiplier = 1d;
    private const double KBytesMultiplier = 1024d;
    private const double MBytesMultiplier = 1024d * 1024;
    private const double GBytesMultiplier = 1024d * 1024 * 1024;
    private const double TBytesMultiplier = 1024d * 1024 * 1024 * 1024;
    private const double PBytesMultiplier = 1024d * 1024 * 1024 * 1024 * 1024;
    private const double EBytesMultiplier = 1024d * 1024 * 1024 * 1024 * 1024 * 1024;
}

/// <summary>
/// Units of data size
/// </summary>
public enum SizeUnit
{
    /// <summary>
    /// B
    /// </summary>
    Bytes,

    /// <summary>
    /// KB
    /// </summary>
    Kilobytes,

    /// <summary>
    /// MB
    /// </summary>
    Megabytes,

    /// <summary>
    /// GB
    /// </summary>
    Gigabytes,

    /// <summary>
    /// TB
    /// </summary>
    Terabytes,

    /// <summary>
    /// PB
    /// </summary>
    Petabyte,

    /// <summary>
    /// EB
    /// </summary>
    Exabyte
}
