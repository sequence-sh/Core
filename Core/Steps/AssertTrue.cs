using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Returns an error if the nested step does not return true.
/// </summary>
[SCLExample("AssertTrue ((2 + 2) == 4)")]
public sealed class AssertTrue : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return await Boolean.Run(stateMonad, cancellationToken)
            .Ensure(
                x => x,
                new SingleError(
                    new ErrorLocation(this),
                    ErrorCode.AssertionFailed,
                    Boolean.Name
                )
            )
            .Map(_ => Unit.Default);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<AssertTrue, Unit>();

    /// <summary>
    /// The bool to test.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<bool> Boolean { get; set; } = null!;
}
