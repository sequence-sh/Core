namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Increment an integer variable by a particular amount
/// </summary>
[SCLExample(
    "- <var> = 1\n- IncrementVariable Variable: <var> Amount: 2\n- Log <var>",
    ExpectedLogs = new[] { "3" }
)]
[SCLExample(
    "- <var> = 1\n- Increment <var> By: 2\n- Log <var>",
    ExpectedLogs = new[] { "3" }
)]
[Alias("Increment")]
public sealed class IncrementVariable : CompoundStep<Unit>
{
    /// <summary>
    /// The variable to increment.
    /// </summary>
    [VariableName(1)]
    [Required]
    public VariableName Variable { get; set; }

    /// <summary>
    /// The amount to increment by.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("1")]
    [Alias("By")]
    public IStep<SCLInt> Amount { get; set; } = new SCLConstant<SCLInt>(1.ConvertToSCLObject());

    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var variable = stateMonad.GetVariable<SCLInt>(Variable).MapError(x => x.WithLocation(this));

        if (variable.IsFailure)
            return variable.ConvertFailure<Unit>();

        var amount = await Amount.Run(stateMonad, cancellationToken);

        if (amount.IsFailure)
            return amount.ConvertFailure<Unit>();

        var r = await stateMonad.SetVariableAsync(
            Variable,
            (variable.Value.Value + amount.Value.Value).ConvertToSCLObject(),
            false,
            this,
            cancellationToken
        );

        return r;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => IncrementVariableStepFactory.Instance;

    /// <summary>
    /// Increment an integer variable by a particular amount
    /// </summary>
    private sealed class IncrementVariableStepFactory : SimpleStepFactory<IncrementVariable, Unit>
    {
        private IncrementVariableStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<IncrementVariable, Unit> Instance { get; } =
            new IncrementVariableStepFactory();

        /// <inheritdoc />
        public override IEnumerable<UsedVariable> GetVariablesUsed(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var vn = freezableStepData.TryGetVariableName(
                nameof(Variable),
                StepType
            );

            if (vn.IsFailure)
                yield break;

            yield return new(
                vn.Value,
                TypeReference.Actual.Integer,
                false,
                freezableStepData.Location
            );
        }
    }
}
