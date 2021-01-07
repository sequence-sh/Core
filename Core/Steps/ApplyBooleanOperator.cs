using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Returns true if both operands are true
/// </summary>
public sealed class ApplyBooleanOperator : CompoundStep<bool>
{
    /// <summary>
    /// The left operand. Will always be evaluated.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<bool> Left { get; set; } = null!;

    /// <summary>
    /// The operator to apply.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<BooleanOperator> Operator { get; set; } = null!;

    /// <summary>
    /// The right operand. Will not be evaluated unless necessary.
    /// </summary>
    [StepProperty(3)]
    [Required]
    public IStep<bool> Right { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<bool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var l = await Left.Run(stateMonad, cancellationToken);

        if (l.IsFailure)
            return l;

        var op = await Operator.Run(stateMonad, cancellationToken);

        if (op.IsFailure)
            return op.ConvertFailure<bool>();

        switch (op.Value)
        {
            case BooleanOperator.And:
            {
                if (l.Value == false)
                    return false;

                var r = await Right.Run(stateMonad, cancellationToken);
                return r;
            }
            case BooleanOperator.Or:
            {
                if (l.Value)
                    return true;

                var r = await Right.Run(stateMonad, cancellationToken);
                return r;
            }

            default:
                return new SingleError(
                    new StepErrorLocation(this),
                    ErrorCode.UnexpectedEnumValue,
                    nameof(Operator),
                    op.Value
                );
        }
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ApplyBooleanOperatorStepFactory.Instance;
}

/// <summary>
/// Returns true if both operands are true
/// </summary>
public sealed class ApplyBooleanOperatorStepFactory : SimpleStepFactory<ApplyBooleanOperator, bool>
{
    private ApplyBooleanOperatorStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static StepFactory Instance { get; } = new ApplyBooleanOperatorStepFactory();

    /// <inheritdoc />
    public override IStepSerializer Serializer => new StepSerializer(
        TypeName,
        new StepComponent(nameof(ApplyBooleanOperator.Left)),
        SpaceComponent.Instance,
        new EnumDisplayComponent<BooleanOperator>(nameof(ApplyBooleanOperator.Operator)),
        SpaceComponent.Instance,
        new StepComponent(nameof(ApplyBooleanOperator.Right))
    );
}

}
