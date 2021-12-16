namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Negation of a boolean value.
/// </summary>
[SCLExample("Not true",  "False")]
[SCLExample("Not false", "True")]
public sealed class Not : CompoundStep<SCLBool>
{
    /// <inheritdoc />
    protected override async Task<Result<SCLBool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return await Boolean.Run(stateMonad, cancellationToken)
            .Map(x => (!x.Value).ConvertToSCLObject());
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => NotStepFactory.Instance;

    /// <summary>
    /// The value to negate.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<SCLBool> Boolean { get; set; } = null!;

    /// <summary>
    /// Negation of a boolean value.
    /// </summary>
    private class NotStepFactory : SimpleStepFactory<Not, SCLBool>
    {
        private NotStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new NotStepFactory();

        ///// <inheritdoc /> //TODO uncomment
        //public override IStepSerializer Serializer =>
        //    new StepSerializer(TypeName, new FixedStringComponent("not"),
        //    new FixedStringComponent("("),
        //    new StepComponent(nameof(Not.Boolean)),
        //    new FixedStringComponent(")")
        //);
    }
}
