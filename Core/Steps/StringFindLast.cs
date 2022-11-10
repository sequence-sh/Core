namespace Sequence.Core.Steps;

/// <summary>
/// Gets the last instance of substring in a string.
/// The index starts at 0.
/// Returns -1 if the substring is not present
/// </summary>
[Alias("LastIndexOfSubstring")]
[Alias("FindLastSubstring")]
[Alias("FindLastInstance")]
[SCLExample("StringFindLast SubString: 'ello' InString: 'hello hello!'", "7")]
[SCLExample("FindLastInstance Of: 'ello' In: 'hello hello!'",            "7")]
[AllowConstantFolding]
public sealed class StringFindLast : CompoundStep<SCLInt>
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
    protected override async ValueTask<Result<SCLInt, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var str = await String.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (str.IsFailure)
            return str.ConvertFailure<SCLInt>();

        var subString = await SubString.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (subString.IsFailure)
            return subString.ConvertFailure<SCLInt>();

        return str.Value.LastIndexOf(subString.Value, StringComparison.Ordinal)
            .ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringFindLast, SCLInt>();
}
