﻿using System;
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
/// Returns true if the String contains the substring.
/// </summary>
[Alias("DoesString")]
[Alias("DoesStringContain")]
[SCLExample("StringContains String: 'hello there' Substring: 'there'", "True")]
[SCLExample("StringContains String: 'hello there' Substring: 'world'", "False")]
[SCLExample("DoesString 'hello there' Contain: 'ello'",                "True")]
public sealed class StringContains : CompoundStep<bool>
{
    /// <inheritdoc />
    protected override async Task<Result<bool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var superstringResult = await String.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (superstringResult.IsFailure)
            return superstringResult.ConvertFailure<bool>();

        var substringResult = await Substring.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (substringResult.IsFailure)
            return substringResult.ConvertFailure<bool>();

        var ignoreCaseResult = await IgnoreCase.Run(stateMonad, cancellationToken);

        if (ignoreCaseResult.IsFailure)
            return ignoreCaseResult.ConvertFailure<bool>();

        var comparison = ignoreCaseResult.Value
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        var r = superstringResult.Value.Contains(substringResult.Value, comparison);

        return r;
    }

    /// <summary>
    /// The superstring to check
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The substring to find
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("Contain")]
    public IStep<StringStream> Substring { get; set; } = null!;

    /// <summary>
    /// Whether to ignore case when comparing strings.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("False")]
    public IStep<bool> IgnoreCase { get; set; } = new BoolConstant(false);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringContains, bool>();
}

}
