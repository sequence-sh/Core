using Namotion.Reflection;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Gets the value of a property from an entity
/// </summary>
[Alias("From")]
[SCLExample("(foo: 123)['foo']",                           "123")]
[SCLExample("(foo: 123).foo",                              "123")]
[SCLExample("(foo: (bar: 123))['foo.bar']",                "123")]
[SCLExample("(foo: (bar: 123)).foo.bar",                   "123")]
[SCLExample("From ('type': 'C', 'value': 1) Get: 'value'", "1")]
[SCLExample(
    "- <myVar> = ('a':[1,2,3])['a']\r\n- <myVar> | Foreach (log (<>))",
    IncludeInDocumentation = false,
    ExpectedLogs = new[] { "1", "2", "3" }
)]
[AllowConstantFolding]
public sealed class EntityGetValue<T> : CompoundStep<T> where T : ISCLObject
{
    /// <inheritdoc />
    protected override Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return Run<T>(stateMonad, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<Result<T1, IError>> Run<T1>(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(
            Entity,
            Property.WrapStringStream(),
            cancellationToken
        );

        if (r.IsFailure)
            return r.ConvertFailure<T1>();

        var (entity, property) = r.Value;

        EntityPropertyKey epk;

        if (property.StartsWith("$"))
        {
            epk = new EntityPropertyKey(property.TrimStart('$'));
        }
        else
        {
            var keys = property.Split(".");
            epk = new EntityPropertyKey(keys);
        }

        var entityValue = entity.TryGetValue(epk);

        if (entityValue.HasNoValue)
            return ISCLObject.GetDefaultValue<T1>();

        var result = entityValue.GetValueOrThrow()
            .TryConvertTyped<T1>(typeof(T1).GetDisplayName())
            .MapError(x => x.WithLocation(this));

        if (result.IsFailure)
        {
            //Special case - allow conversion to stringstream
            var ss = new StringStream(
                entityValue.GetValueOrThrow().Serialize(SerializeOptions.Serialize)
            );

            if (ss is T1 t1ss)
                return t1ss;
        }

        return result;
    }

    /// <summary>
    /// The entity to get the property from.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <summary>
    /// The name of the property to get.
    /// Returns an empty string if the property is not present.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("Get")]
    public IStep<StringStream> Property { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => EntityGetValueStepFactory.Instance;

    /// <inheritdoc />
    public override bool ShouldBracketWhenSerialized => false;

    private sealed class EntityGetValueStepFactory : GenericStepFactory
    {
        private EntityGetValueStepFactory() { }
        public static GenericStepFactory Instance { get; } = new EntityGetValueStepFactory();

        /// <inheritdoc />
        public override Type StepType { get; } = typeof(EntityGetValue<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "The required type";

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var entityStep = freezableStepData.TryGetStep(
                nameof(EntityGetValue<ISCLObject>.Entity),
                StepType
            );

            if (entityStep.IsFailure)
                return entityStep.ConvertFailure<TypeReference>();

            var propertyStep = freezableStepData.TryGetStep(
                nameof(EntityGetValue<ISCLObject>.Property),
                StepType
            );

            if (propertyStep.IsFailure)
                return entityStep.ConvertFailure<TypeReference>();

            var entityTypeReference =
                entityStep.Value.TryGetOutputTypeReference(callerMetadata, typeResolver);

            if (entityTypeReference.IsSuccess)
            {
                var etrNode = entityTypeReference.Value.ToSchemaNode(typeResolver);

                if (etrNode is EntityNode currentNode)
                {
                    var frozenPropertyStep = propertyStep.Value.TryFreeze(
                        new CallerMetadata(
                            TypeName,
                            nameof(EntityGetValue<ISCLObject>.Property),
                            TypeReference.Actual.String
                        ),
                        typeResolver.StepFactoryStore
                    );

                    if (frozenPropertyStep.IsSuccess)
                    {
                        if (frozenPropertyStep.Value.HasConstantValue(
                                ArraySegment<VariableName>.Empty
                            ))
                        {
                            var propNameMaybe =
                                frozenPropertyStep.Value.TryGetConstantValueAsync(
                                        ImmutableDictionary<VariableName, ISCLObject>.Empty,
                                        typeResolver.StepFactoryStore
                                    )
                                    .Result;

                            if (propNameMaybe.HasValue
                             && propNameMaybe.Value is StringStream propName)
                            {
                                var epk = EntityPropertyKey.Create(propName.GetString());

                                var typeReference = currentNode.GetPropertyTypeReference(epk);

                                if (typeReference.HasNoValue)
                                    return ErrorCode.EntityPropertyNever
                                        .ToErrorBuilder(propName.GetString())
                                        .WithLocationSingle(freezableStepData.Location);

                                var combinedType =
                                    callerMetadata.ExpectedType.TryCombine(
                                        typeReference.Value,
                                        typeResolver
                                    );

                                if (combinedType.IsSuccess)
                                {
                                    if (combinedType.Value.IsUnknown || combinedType.Value
                                     == TypeReference.Any.Instance)
                                        return TypeReference.Dynamic.Instance;

                                    return combinedType.Value;
                                }

                                return ErrorCode.WrongType.ToErrorBuilder(
                                        callerMetadata.StepName,
                                        callerMetadata.ExpectedType.Name,
                                        callerMetadata.ParameterName,
                                        $"Entity['{propName.GetString()}']",
                                        typeReference.Value.Name
                                    )
                                    .WithLocationSingle(freezableStepData.Location);
                            }
                        }
                    }
                }
            }

            //We are unable to get full type information. Resolve dynamically

            if (callerMetadata.ExpectedType.IsUnknown
             || callerMetadata.ExpectedType == TypeReference.Any.Instance)
            {
                return TypeReference.Dynamic.Instance;
            }

            return callerMetadata.ExpectedType;
        }

        /// <inheritdoc />
        public override TypeReference GetOutputTypeReference(TypeReference memberTypeReference)
        {
            return memberTypeReference;
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer => EntityGetValueSerializer.Instance;
    }
}
