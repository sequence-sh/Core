using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets a substring from a string.
/// </summary>
public sealed class GetSubstring : CompoundStep<StringStream>
{
    /// <summary>
    /// The string to extract a substring from.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The index.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("0")]
    public IStep<int> Index { get; set; } = new IntConstant(0);

    /// <summary>
    /// The length of the substring to extract.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<int> Length { get; set; } = null!;

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

        var resultString = str.Substring(index.Value, length.Value);

        return new StringStream(resultString);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<GetSubstring, StringStream>();
}

}
