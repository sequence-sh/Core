namespace Sequence.Core.Steps;

/// <summary>
/// Rename entity properties.
/// </summary>
[Alias("RenameEntityFields")]
[Alias("RenameProperties")]
[Alias("RenameProps")]
[SCLExample(
    @"EntityMapProperties In: [
  ('typeA': 'A', 'valueA': 1)
  ('typeB': 'B', 'valueB': 2)
  ('typeA': 'A', 'valueA': 3)
] Mappings: ('value': ['valueA', 'valueB'] 'type': ['typeA', 'typeB'])",
    "[('type': \"A\" 'value': 1), ('type': \"B\" 'value': 2), ('type': \"A\" 'value': 3)]"
)]
[SCLExample(
    "RenameProperties In: [('a': 1), ('b': 1), ('c': 1)] To: ('value': ['a', 'b', 'c'])",
    "[('value': 1), ('value': 1), ('value': 1)]"
)]
[AllowConstantFolding]
public class EntityMapProperties : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(EntityStream, Mappings, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Array<Entity>>();

        var (entityStream, mappingsEntity) = r.Value;

        var mappings = mappingsEntity
            .Select(
                x => new KeyValuePair<EntityKey, IReadOnlyList<EntityNestedKey>>(
                    x.Key,
                    GetStringList(x.Value)
                )
            )
            .ToList();

        if (mappings.All(
                x => x.Value.Count == 1 && x.Value.Single().KeyNames.Count == 1
            )) //Simple case of just column renames
        {
            var simpleMappings = mappings.Select(x => (x.Key, x.Value.Single().KeyNames.Single()))
                .ToList();

            var headersCache = new HeadersCache();

            var newEntityStream = entityStream
                .Select(e => ChangePropertyNamesSimple(e, simpleMappings, headersCache));

            return newEntityStream;
        }
        else
        {
            var newEntityStream = entityStream
                .Select(e => ChangePropertyNames(e, mappings));

            return newEntityStream;
        }

        static Entity ChangePropertyNamesSimple(
            Entity oldEntity,
            IReadOnlyList<(EntityKey newKey, EntityKey oldKey)> mappings,
            HeadersCache headersCache)
        {
            if (headersCache.Data.HasValue
             && (headersCache.Data.Value.oldHeaders == oldEntity.Headers
              || headersCache.Data.Value.oldHeaders.SequenceEqual(oldEntity.Headers)))
            {
                var newEntity = oldEntity with { Headers = headersCache.Data.Value.newHeaders };
                return newEntity;
            }
            else
            {
                var newEntity = oldEntity;

                foreach (var (newKey, oldKey) in mappings)
                {
                    var ne = newEntity.WithPropertyRenamed(oldKey, newKey);

                    if (ne.HasValue)
                        newEntity = ne.Value;
                }

                headersCache.Data =
                    Maybe<(ImmutableArray<EntityKey> oldHeaders, ImmutableArray<EntityKey>
                        newHeaders)>.From((oldEntity.Headers, newEntity.Headers));

                return newEntity;
            }
        }

        static Entity ChangePropertyNames(
            Entity oldEntity,
            IReadOnlyList<KeyValuePair<EntityKey, IReadOnlyList<EntityNestedKey>>> mappings)
        {
            var newEntity = oldEntity;

            foreach (var (newName, propertyKeys) in mappings)
            {
                var done = false;

                foreach (var entityPropertyKey in propertyKeys)
                {
                    if (done)
                    {
                        newEntity = newEntity.WithNestedPropertyRemoved(entityPropertyKey)
                            .GetValueOrDefault(newEntity);
                    }
                    else if (entityPropertyKey.KeyNames.Count
                          == 1) //simply rename an existing column
                    {
                        var renamed = newEntity.WithPropertyRenamed(
                            entityPropertyKey.KeyNames.Single(),
                            newName
                        );

                        if (renamed.HasValue)
                        {
                            done      = true;
                            newEntity = renamed.Value;
                        }
                    }
                    else //remap a nested property - the new column will be appended
                    {
                        var value = newEntity.TryGetProperty(entityPropertyKey);

                        if (value.HasValue)
                        {
                            done = true;
                            var newProperty = value.GetValueOrThrow();

                            var index =
                                newEntity.Headers.IndexOf(entityPropertyKey.KeyNames.First());

                            newEntity = newEntity.WithPropertyAdded(
                                newName,
                                newProperty.Value,
                                index
                            );

                            newEntity = newEntity.WithNestedPropertyRemoved(entityPropertyKey)
                                .GetValueOrDefault(newEntity);
                        }
                    }
                }
            }

            return newEntity;
        }

        static IReadOnlyList<EntityNestedKey> GetStringList(ISCLObject ev)
        {
            if (ev is IArray nestedList)
            {
                return nestedList.ListIfEvaluated()
                    .Value
                    .Select(x => EntityNestedKey.Create(x.Serialize(SerializeOptions.Primitive)))
                    .ToList();
            }

            return new[] { EntityNestedKey.Create(ev.Serialize(SerializeOptions.Primitive)) };
        }
    }

    /// <summary>
    /// The stream of entities to change the field names of.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("In")]
    public IStep<Array<Entity>> EntityStream { get; set; } = null!;

    /// <summary>
    /// An entity containing mappings.
    /// The keys in the entity will be the new column names.
    /// The value of each property should either the name of the source column or a list of source column names
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("To")]
    public IStep<Entity> Mappings { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityMapProperties, Array<Entity>>();

    /// <summary>
    /// Caches headers for simple entity renames
    /// </summary>
    private class HeadersCache
    {
        public Maybe<(ImmutableArray<EntityKey> oldHeaders, ImmutableArray<EntityKey> newHeaders)>
            Data
        {
            get;
            set;
        }
    }
}
