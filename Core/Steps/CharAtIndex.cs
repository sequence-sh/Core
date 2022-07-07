namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Gets the letter that appears at a specific index.
/// First letter is at index 0.
/// </summary>
[Alias("GetLetter")]
[SCLExample("CharAtIndex 'hello' 1",             "e")]
[SCLExample("GetLetter From: 'Bye!' AtIndex: 0", "B")]
[AllowConstantFolding]
public sealed class CharAtIndex : CompoundStep<StringStream>
{
    /// <summary>
    /// The string to extract a substring from.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("From")]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The index.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("AtIndex")]
    public IStep<SCLInt> Index { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(
            String.WrapStringStream(),
            Index,
            cancellationToken
        );

        if (r.IsFailure)
            return r.ConvertFailure<StringStream>();

        var (str, index) = r.Value;

        if (index.Value < 0 || index.Value >= str.Length)
            return new SingleError(new ErrorLocation(this), ErrorCode.IndexOutOfBounds);

        var character = str[index.Value].ToString();

        return new StringStream(character);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<CharAtIndex, StringStream>();
}
