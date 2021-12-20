namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Returns a copy of the entity with this property set.
/// Will add a new property if the property is not already present.
/// </summary>
[Alias("In")]
[SCLExample(
    "EntitySetValue Entity: ('type': 'A', 'value': 1) Property: 'type' Value: 'C'",
    "('type': \"C\" 'value': 1)"
)]
[SCLExample("In ('type': 'C', 'value': 1) Set: 'type' To: 'A'", "('type': \"A\" 'value': 1)")]
public sealed class EntitySetValue<T> : CompoundStep<Entity> where T : ISCLObject
{
    /// <inheritdoc />
    protected override async Task<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entityResult = await Entity.Run(stateMonad, cancellationToken);

        if (entityResult.IsFailure)
            return entityResult;

        var propertyResult = await Property.Run(stateMonad, cancellationToken);

        if (propertyResult.IsFailure)
            return propertyResult.ConvertFailure<Entity>();

        var valueResult = await Value.Run(stateMonad, cancellationToken)
            .Bind(x => EntityHelper.TryUnpackObjectAsync(x, cancellationToken));

        if (valueResult.IsFailure)
            return valueResult.ConvertFailure<Entity>();

        var propertyName = await propertyResult.Value.GetStringAsync();

        var newEntity = entityResult.Value.WithProperty(propertyName, valueResult.Value, null);

        return newEntity;
    }

    /// <summary>
    /// The entity to set the property on.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <summary>
    /// The name of the property to set.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("Set")]
    public IStep<StringStream> Property { get; set; } = null!;

    /// <summary>
    /// The new value of the property to set.
    /// </summary>
    [StepProperty(3)]
    [Required]
    [Alias("To")]
    public IStep<T> Value { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => EntitySetValueStepFactory.Instance;

    /// <summary>
    /// Returns a copy of the entity with this property set.
    /// Will add a new property if the property is not already present.
    /// </summary>
    private sealed class EntitySetValueStepFactory : GenericStepFactory
    {
        private EntitySetValueStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new EntitySetValueStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(EntitySetValue<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Entity);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            TypeReference.Actual.Entity;

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var allowResult = callerMetadata.CheckAllows(TypeReference.Actual.Entity, typeResolver)
                .MapError(x => x.WithLocation(freezableStepData));

            if (allowResult.IsFailure)
                return allowResult.ConvertFailure<TypeReference>();

            var r1 = freezableStepData.TryGetStep(
                    nameof(EntitySetValue<ISCLObject>.Value),
                    StepType
                )
                .Bind(
                    x => x.TryGetOutputTypeReference(
                        new CallerMetadata(
                            TypeName,
                            nameof(EntitySetValue<ISCLObject>.Value),
                            TypeReference.Any.Instance
                        ),
                        typeResolver
                    )
                );

            return r1;
        }
    }
}
