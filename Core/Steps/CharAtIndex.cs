﻿using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets the letters that appears at a specific index
/// </summary>
[SCLExample("CharAtIndex 'hello' 1", "e")]
public sealed class CharAtIndex : CompoundStep<StringStream>
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
    [StepProperty(2)]
    [Required]
    public IStep<int> Index { get; set; } = null!;

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

        if (index < 0 || index >= str.Length)
            return new SingleError(new ErrorLocation(this), ErrorCode.IndexOutOfBounds);

        var character = str[index].ToString();

        return new StringStream(character);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<CharAtIndex, StringStream>();
}

}
