namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Group elements in an array or entities in a stream using a function.
/// Entities in the resulting array will have two properties `Key` and `Values`.
/// The `Key` will be set according to the result of the function.
/// </summary>
[SCLExample(
    @"Group Array: [
  ('type': 'A', 'value': 1)
  ('type': 'B', 'value': 2)
  ('type': 'A', 'value': 3)
] Using: (<item>['type'])",
    "[('Key': \"A\" 'Values': [('type': \"A\" 'value': 1), ('type': \"A\" 'value': 3)]), ('Key': \"B\" 'Values': [('type': \"B\" 'value': 2)])]"
)]
[Alias("Group")]
public sealed class ArrayGroupBy<T> : CompoundStep<Array<Entity>> where T : ISCLObject
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var arrayResult = await Array.Run(stateMonad, cancellationToken);

        if (arrayResult.IsFailure)
            return arrayResult.ConvertFailure<Array<Entity>>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async ValueTask<StringStream> Action(T record)
        {
            await using var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                Function.VariableNameOrItem,
                new KeyValuePair<VariableName, ISCLObject>(Function.VariableNameOrItem, record!)
            );

            var result = await Function.StepTyped.Run(scopedMonad, cancellationToken);

            if (result.IsFailure)
                throw new ErrorException(result.Error);

            return result.Value;
        }

        var newStream = arrayResult.Value.GroupByAwait(Action);

        return newStream;
    }

    /// <summary>
    /// The array or entity stream to group
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// A function to use to group entities
    /// </summary>
    [FunctionProperty(2)]
    [Required]
    [Alias("By")]
    [Alias("Using")]
    public LambdaFunction<T, StringStream> Function { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayGroupByStepFactory.Instance;

    private sealed class ArrayGroupByStepFactory : ArrayStepFactory
    {
        private ArrayGroupByStepFactory() { }
        public static ArrayStepFactory Instance { get; } = new ArrayGroupByStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayGroupBy<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of Entity";

        /// <inheritdoc />
        protected override TypeReference GetOutputTypeReference(TypeReference memberTypeReference)
        {
            return new TypeReference.Array(TypeReference.Actual.Entity);
        }

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            var r = callerMetadata.CheckAllows(
                new TypeReference.Array(TypeReference.Actual.Entity),
                null
            );

            if (r.IsFailure)
                return r.ConvertFailure<TypeReference>();

            return new TypeReference.Array(TypeReference.Any.Instance);
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayGroupBy<ISCLObject>.Array);

        protected override string LambdaPropertyName => nameof(ArrayGroupBy<ISCLObject>.Function);
    }
}
