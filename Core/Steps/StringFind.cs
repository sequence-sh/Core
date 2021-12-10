namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Gets the index of the first instance of a substring in a string.
/// The index starts at 0.
/// Returns -1 if the substring is not present.
/// </summary>
[Alias("IndexOfSubstring")]
[Alias("FindInstance")]
[SCLExample("StringFind SubString: 'ello' InString: 'hello hello!'", "1")]
[SCLExample("FindInstance Of: 'ello' In: 'hello hello!'",            "1")]
public sealed class StringFind : CompoundStep<SCLInt>
{
    /// <summary>
    /// The string to check.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("In")]
    [Alias("InString")]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The substring to find.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("Of")]
    public IStep<StringStream> SubString { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<int, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var str = await String.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (str.IsFailure)
            return str.ConvertFailure<int>();

        var subString = await SubString.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (subString.IsFailure)
            return subString.ConvertFailure<int>();

        return str.Value.IndexOf(subString.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<StringFind, int>();
}
