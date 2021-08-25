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
/// Converts a string to an integer
/// </summary>
[Alias("ToInt")]
[SCLExample("StringToInt '123'", "123")]
public sealed class StringToInt : CompoundStep<int>
{
    /// <summary>
    /// The string to convert to an integer
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("String")]
    public IStep<StringStream> Integer { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<int, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await Integer.WrapStringStream().Run(stateMonad, cancellationToken);

        if (result.IsFailure)
            return result.ConvertFailure<int>();

        if (int.TryParse(result.Value, out var i))
        {
            return i;
        }

        return ErrorCode.CouldNotParse.ToErrorBuilder(result.Value, SCLType.Integer.ToString())
            .WithLocationSingle(this);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<StringToInt, int>();
}

}
