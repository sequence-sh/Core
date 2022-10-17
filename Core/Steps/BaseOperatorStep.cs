namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Base class for operator operations
/// </summary>
[AllowConstantFolding]
public abstract class BaseOperatorStep<TStep, TElement, TOutput> : CompoundStep<TOutput>
    where TStep : BaseOperatorStep<TStep, TElement, TOutput>, new()
    where TElement : ISCLObject
    where TOutput : ISCLObject

{
    /// <summary>
    /// The terms to operate on
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<TElement>> Terms { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = ChainInfixStepFactory.Instance;

    /// <inheritdoc />
    protected override async ValueTask<Result<TOutput, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var termsResult =
            await Terms.Run(stateMonad, cancellationToken)
                .Bind(x => x.GetElementsAsync(cancellationToken));

        if (termsResult.IsFailure)
            return termsResult.ConvertFailure<TOutput>();

        var result = Operate(termsResult.Value);

        if (result.IsFailure)
            return result.MapError(x => x.WithLocation(this));

        return result.Value;
    }

    /// <summary>
    /// The operator
    /// </summary>
    public abstract string Operator { get; }

    /// <summary>
    /// Calculate the result
    /// </summary>
    protected abstract Result<TOutput, IErrorBuilder> Operate(IEnumerable<TElement> terms);

    /// <summary>
    /// Step factory for chain infix steps
    /// </summary>
    protected class ChainInfixStepFactory : SimpleStepFactory<TStep, TOutput>
    {
        private ChainInfixStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<TStep, TOutput> Instance { get; } =
            new ChainInfixStepFactory();

        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } =
            new ChainInfixSerializer(FormatTypeName(typeof(TStep)), new TStep().Operator);
    }
}
