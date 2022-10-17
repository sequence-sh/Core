namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Appends a string to an existing string variable.
/// </summary>
[SCLExample(
    "- <var> = 'hello'\n- StringAppend <var> ' world'\n- Log <var>",
    null,
    null,
    "hello world"
)]
[Alias("AppendString")]
public sealed class StringAppend : CompoundStep<Unit>
{
    /// <summary>
    /// The variable to append to.
    /// </summary>
    [VariableName(1)]
    [Required]
    public VariableName Variable { get; set; }

    /// <summary>
    /// The string to append.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var str = await String.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (str.IsFailure)
            return str.ConvertFailure<Unit>();

        var currentValue = stateMonad.GetVariable<StringStream>(Variable)
            .MapError(x => x.WithLocation(this));

        if (currentValue.IsFailure)
            return currentValue.ConvertFailure<Unit>();

        var newValue = await currentValue.Value.GetStringAsync() + str.Value;

        var r = await stateMonad.SetVariableAsync(
            Variable,
            new StringStream(newValue),
            false,
            this,
            cancellationToken
        );

        if (r.IsFailure)
            return r.ConvertFailure<Unit>();

        return Unit.Default;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => StringAppendStepFactory.Instance;

    private sealed class StringAppendStepFactory : SimpleStepFactory<StringAppend, Unit>
    {
        private StringAppendStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<StringAppend, Unit> Instance { get; } =
            new StringAppendStepFactory();

        /// <inheritdoc />
        public override IEnumerable<UsedVariable> GetVariablesUsed(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var vn = freezableStepData.TryGetVariableName(nameof(StringAppend.Variable), StepType);

            if (vn.IsFailure)
                yield break;

            yield return new(
                vn.Value,
                TypeReference.Actual.String,
                false,
                freezableStepData.Location
            );
        }
    }
}
