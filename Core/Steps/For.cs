namespace Reductech.Sequence.Core.Steps;

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
    public IStep<SCLInt> From { get; set; } = null!;

    /// <summary>
    /// The highest value of the variable to use
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<SCLInt> To { get; set; } = null!;

    /// <summary>
    /// The amount to increment by each iteration.
    /// </summary>
    [StepProperty(3)]
    [Required]
    public IStep<SCLInt> Increment { get; set; } = null!;

    /// <summary>
    /// The action to perform repeatedly.
    /// </summary>
    [FunctionProperty(4)]
    [Required]
    public LambdaFunction<SCLInt, Unit> Action { get; set; } = null!;

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

        if (increment.Value.Value == 0)
            return new SingleError(new ErrorLocation(this), ErrorCode.DivideByZero);

        var currentValue = from.Value.Value;

        var scopedStateMonad = new ScopedStateMonad(
            stateMonad,
            stateMonad.GetState().ToImmutableDictionary(),
            variableName,
            new KeyValuePair<VariableName, ISCLObject>(
                variableName,
                currentValue.ConvertToSCLObject()
            )
        );

        while (increment.Value.Value > 0
                   ? currentValue <= to.Value.Value
                   : currentValue >= to.Value.Value)
        {
            var r = await Action.StepTyped.Run(scopedStateMonad, cancellationToken);

            if (r.IsFailure)
                return r;

            var currentValueResult = scopedStateMonad.GetVariable<SCLInt>(variableName)
                .MapError(e => e.WithLocation(this));

            if (currentValueResult.IsFailure)
                return currentValueResult.ConvertFailure<Unit>();

            currentValue =  currentValueResult.Value.Value;
            currentValue += increment.Value.Value;

            var setResult2 = await scopedStateMonad.SetVariableAsync(
                variableName,
                currentValue.ConvertToSCLObject(),
                false,
                this,
                cancellationToken
            );

            if (setResult2.IsFailure)
                return setResult2.ConvertFailure<Unit>();
        }

        return Unit.Default;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<For, Unit>();
}
