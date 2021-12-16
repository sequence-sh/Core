namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Base class for compare operations
/// </summary>
public abstract class
    CompareBaseOperatorStep<TStep, TElement> : BaseOperatorStep<TStep, TElement, SCLBool>
    where TStep : BaseOperatorStep<TStep, TElement, SCLBool>, new() where TElement : ISCLObject
{
    /// <summary>
    /// Check the result of comparing a term with the next term
    /// -1 means less than
    /// 0 means equals
    /// 1 means greater than
    /// </summary>
    protected abstract bool CheckComparisonValue(int v);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = CompareOperatorStepFactory.Instance;

    /// <inheritdoc />
    protected override Result<SCLBool, IErrorBuilder> Operate(IEnumerable<TElement> terms)
    {
        var       last     = Maybe<TElement>.None;
        IComparer comparer = Comparer<TElement>.Default;

        if (comparer.Equals(Comparer<object>.Default))
            comparer = DefaultObjectComparer.Instance;

        foreach (var term in terms)
        {
            if (last.HasValue)
            {
                var comparisonValue = comparer.Compare(last.GetValueOrThrow(), term);

                var checkResult = CheckComparisonValue(comparisonValue);

                if (!checkResult)
                    return SCLBool.False;
            }

            last = term;
        }

        return SCLBool.True;
    }

    private class DefaultObjectComparer : IComparer<object>, IComparer
    {
        private DefaultObjectComparer() { }
        public static DefaultObjectComparer Instance { get; } = new();

        public int Compare(object? x, object? y)
        {
            if (x is null || y is null)
                return Comparer<object>.Default.Compare(x, y);

            if (x.GetType() == y.GetType())
            {
                return Comparer<object>.Default.Compare(x, y);
            }

            var xString = x.ToString();
            var yString = y.ToString();
            return Comparer<string>.Default.Compare(xString, yString);
        }
    }

    /// <summary>
    /// Step factory for operators
    /// </summary>
    protected sealed class CompareOperatorStepFactory : GenericStepFactory
    {
        private CompareOperatorStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static CompareOperatorStepFactory Instance { get; } = new();

        /// <inheritdoc />
        public override Type StepType => typeof(TStep).GetGenericTypeDefinition();

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Boolean";

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => TypeReference.Actual.Bool;

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var checkResult = callerMetadata
                .CheckAllows(TypeReference.Actual.Bool, typeResolver)
                .MapError(x => x.WithLocation(freezableStepData));

            if (checkResult.IsFailure)
                return checkResult.ConvertFailure<TypeReference>();

            var result = freezableStepData
                .TryGetStep(nameof(Terms), StepType)
                .Bind(
                    x => x.TryGetOutputTypeReference(
                        new CallerMetadata(
                            TypeName,
                            nameof(Terms),
                            new TypeReference.Array(TypeReference.Any.Instance)
                        ),
                        typeResolver
                    )
                )
                .Bind(
                    x => x.TryGetArrayMemberTypeReference(typeResolver)
                        .MapError(e => e.WithLocation(freezableStepData))
                );

            return result;
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new ChainInfixSerializer(
            FormatTypeName(typeof(TStep)),
            new TStep().Operator
        );
    }
}
