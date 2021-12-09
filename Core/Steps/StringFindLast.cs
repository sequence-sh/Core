using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps;

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
public sealed class StringFindLast : CompoundStep<int>
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

        return str.Value.LastIndexOf(subString.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringFindLast, int>();
}
