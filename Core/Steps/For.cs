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
/// Do an action for each value of &lt;i&gt; in a range.
/// </summary>
public sealed class For : CompoundStep<Unit>
{
    /// <summary>
    /// The first value of the variable to use.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<int> From { get; set; } = null!;

    /// <summary>
    /// The highest value of the variable to use
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<int> To { get; set; } = null!;

    /// <summary>
    /// The amount to increment by each iteration.
    /// </summary>
    [StepProperty(3)]
    [Required]
    public IStep<int> Increment { get; set; } = null!;

    /// <summary>
    /// The action to perform repeatedly.
    /// </summary>
    [StepProperty(4)]
    [Required]
    public LambdaFunction<int, Unit> Action { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var variableName = Action.VariableNameOrItem;

        var from = await From.Run(stateMonad, cancellationToken);

        if (from.IsFailure)
            return from.ConvertFailure<Unit>();

        var to = await To.Run(stateMonad, cancellationToken);

        if (to.IsFailure)
            return to.ConvertFailure<Unit>();

        var increment = await Increment.Run(stateMonad, cancellationToken);

        if (increment.IsFailure)
            return increment.ConvertFailure<Unit>();

        var currentValue = from.Value;

        //Todo usae scoped state monad

        var setResult = await stateMonad.SetVariableAsync(variableName, currentValue, false, this);

        if (setResult.IsFailure)
            return setResult.ConvertFailure<Unit>();

        if (increment.Value == 0)
            return new SingleError(new ErrorLocation(this), ErrorCode.DivideByZero);

        while (increment.Value > 0 ? currentValue <= to.Value : currentValue >= to.Value)
        {
            var r = await Action.StepTyped.Run(stateMonad, cancellationToken);

            if (r.IsFailure)
                return r;

            var currentValueResult = stateMonad.GetVariable<int>(variableName)
                .MapError(e => e.WithLocation(this));

            if (currentValueResult.IsFailure)
                return currentValueResult.ConvertFailure<Unit>();

            currentValue =  currentValueResult.Value;
            currentValue += increment.Value;

            var setResult2 = await stateMonad.SetVariableAsync(
                variableName,
                currentValue,
                false,
                this
            );

            if (setResult2.IsFailure)
                return setResult.ConvertFailure<Unit>();
        }

        await stateMonad.RemoveVariableAsync(VariableName.Item, false, this);

        return Unit.Default;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<For, Unit>();
}

}
