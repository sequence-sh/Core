namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Checks if a step returns null
/// </summary>
[SCLExample("IsNull Null",           "True")]
[SCLExample("IsNull 1",              "False")]
[SCLExample("IsNull (a: Null)['a']", "True")]
[SCLExample("IsNull (a: 1)['a']",    "False")]
public sealed class IsNull<T> : CompoundStep<bool>
{
    /// <inheritdoc />
    protected override async Task<Result<bool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await Value.Run(stateMonad, cancellationToken);

        if (result.IsFailure)
            return result.ConvertFailure<bool>();

        if (result.Value is SCLNull)
            return true;

        return false;
    }

    /// <summary>
    /// The value to check for null
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<T> Value { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = IsNullStepFactory.Instance;

    /// <inheritdoc />
    private class IsNullStepFactory : GenericStepFactory
    {
        private IsNullStepFactory() { }
        public static GenericStepFactory Instance { get; } = new IsNullStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(IsNull<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Boolean);

        /// <inheritdoc />
        protected override TypeReference GetOutputTypeReference(TypeReference memberTypeReference)
        {
            return TypeReference.Actual.Bool;
        }

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var step = freezableStepData.TryGetStep(nameof(IsNull<object>.Value), StepType);

            if (step.IsFailure)
                return step.ConvertFailure<TypeReference>();

            var typeReference = step.Value.TryGetOutputTypeReference(callerMetadata, typeResolver);

            return typeReference;
        }
    }
}
