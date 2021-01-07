using System;
using System.ComponentModel.DataAnnotations;
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
/// Trims a string.
/// </summary>
public sealed class StringTrim : CompoundStep<StringStream>
{
    /// <summary>
    /// The string to change the case of.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The side to trim.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("Both")]
    public IStep<TrimSide> Side { get; set; } = new EnumConstant<TrimSide>(TrimSide.Both);

    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stringResult = await String.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (stringResult.IsFailure)
            return stringResult.ConvertFailure<StringStream>();

        var sideResult = await Side.Run(stateMonad, cancellationToken);

        if (sideResult.IsFailure)
            return sideResult.ConvertFailure<StringStream>();

        var r = TrimString(stringResult.Value, sideResult.Value);

        return new StringStream(r);
    }

    private static string TrimString(string s, TrimSide side) => side switch
    {
        TrimSide.Start => s.TrimStart(),
        TrimSide.End   => s.TrimEnd(),
        TrimSide.Both  => s.Trim(),
        _              => throw new ArgumentOutOfRangeException(nameof(side), side, null)
    };

    /// <inheritdoc />
    public override IStepFactory StepFactory => StringTrimStepFactory.Instance;
}

/// <summary>
/// Trims a string.
/// </summary>
public sealed class StringTrimStepFactory : SimpleStepFactory<StringTrim, StringStream>
{
    private StringTrimStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static SimpleStepFactory<StringTrim, StringStream> Instance { get; } =
        new StringTrimStepFactory();
}

}
