namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Gets a substring from a string.
/// </summary>
[Alias("GetSubstring")]
[SCLExample("StringSubstring 'hello world!' Index: 6 Length: 5", "world")]
[SCLExample("GetSubstring From: 'hello world!' Length: 5",       "hello")]
public sealed class StringSubstring : CompoundStep<StringStream>
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
    [StepProperty(3)]
    [DefaultValueExplanation("0")]
    public IStep<SCLInt> Index { get; set; } = new IntConstant(0);

    /// <summary>
    /// The length of the substring to extract.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<SCLInt> Length { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stringResult = await String.Run(stateMonad, cancellationToken);

        if (stringResult.IsFailure)
            return stringResult;

        var index = await Index.Run(stateMonad, cancellationToken);

        if (index.IsFailure)
            return index.ConvertFailure<StringStream>();

        var length = await Length.Run(stateMonad, cancellationToken);

        if (length.IsFailure)
            return length.ConvertFailure<StringStream>();

        var str = await stringResult.Value.GetStringAsync();

        if (index.Value < 0 || index.Value >= str.Length)
            return new SingleError(new ErrorLocation(this), ErrorCode.IndexOutOfBounds);

        StringStream resultString = str.Substring(index.Value, length.Value);

        return resultString;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringSubstring, StringStream>();
}
