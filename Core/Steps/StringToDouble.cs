using System.ComponentModel.DataAnnotations;
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
/// Converts a string to a double
/// </summary>
[Alias("ToDouble")]
[SCLExample("StringToDouble '123.45'", "123.45")]
public sealed class StringToDouble : CompoundStep<double>
{
    /// <summary>
    /// The string to convert to an integer
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("String")]
    public IStep<StringStream> Double { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<double, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await Double.WrapStringStream().Run(stateMonad, cancellationToken);

        if (result.IsFailure)
            return result.ConvertFailure<double>();

        if (double.TryParse(result.Value, out var i))
        {
            return i;
        }

        return ErrorCode.CouldNotParse.ToErrorBuilder(result.Value, SCLType.Double.ToString())
            .WithLocationSingle(this);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringToDouble, double>();
}

}
