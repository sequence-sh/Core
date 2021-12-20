namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Gets the value of a named variable.
/// </summary>
public sealed class GetVariable<T> : CompoundStep<T> where T : ISCLObject
{
    /// <inheritdoc />
    #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    protected override async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) =>
        stateMonad.GetVariable<T>(Variable).MapError(x => x.WithLocation(this));
    #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    /// <inheritdoc />
    public override IStepFactory StepFactory => GetVariableStepFactory.Instance;

    /// <summary>
    /// The name of the variable to get.
    /// </summary>
    [VariableName(1)]
    [Required]
    public VariableName Variable { get; set; }

    /// <inheritdoc />
    public override string Name => Variable == default
        ? base.Name
        : $"Get {Variable.Serialize(SerializeOptions.Name)}";

    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    private class GetVariableStepFactory : GenericStepFactory
    {
        private GetVariableStepFactory() { }

        /// <summary>
        /// The Instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new GetVariableStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(GetVariable<>);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var variableName = freezableStepData
                .TryGetVariableName(nameof(GetVariable<ISCLObject>.Variable), StepType);

            if (variableName.IsFailure)
                return variableName.ConvertFailure<TypeReference>();

            var expectedTypeReference = callerMetadata.ExpectedType;

            if (typeResolver.Dictionary.TryGetValue(variableName.Value, out var tr))
            {
                var result = tr.TryCombine(expectedTypeReference, typeResolver);
                return result.MapError(x => x.WithLocation(freezableStepData));
            }

            return new TypeReference.Variable(variableName.Value);
        }

        /// <inheritdoc />
        public override IEnumerable<UsedVariable> GetVariablesUsed(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var vn = freezableStepData.TryGetVariableName(
                nameof(GetVariable<ISCLObject>.Variable),
                StepType
            );

            if (vn.IsFailure)
                yield break;

            yield return new(vn.Value, callerMetadata.ExpectedType, freezableStepData.Location);
        }

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        public override IStepSerializer Serializer => new StepSerializer(
            TypeName,
            new StepComponent(nameof(GetVariable<ISCLObject>.Variable))
        );
    }
}
