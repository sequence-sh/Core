using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Converts a string to a particular case.
/// </summary>
public sealed class StringToCase : CompoundStep<StringStream>
{
    /// <summary>
    /// The string to change the case of.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The case to change to.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<TextCase> Case { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stringResult = await String.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (stringResult.IsFailure)
            return stringResult.ConvertFailure<StringStream>();

        var caseResult = await Case.Run(stateMonad, cancellationToken);

        if (caseResult.IsFailure)
            return caseResult.ConvertFailure<StringStream>();

        StringStream r = Convert(stringResult.Value, caseResult.Value);

        return r;
    }

    private static string Convert(string s, TextCase textCase) => textCase switch
    {
        TextCase.Upper => s.ToUpperInvariant(),
        TextCase.Lower => s.ToLowerInvariant(),
        TextCase.Title => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s),
        _              => throw new ArgumentOutOfRangeException(nameof(textCase), textCase, null)
    };

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringToCase, StringStream>();
}

}
