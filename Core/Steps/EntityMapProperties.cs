namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Rename entity properties
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
public class EntityMapProperties : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(EntityStream, Mappings, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Array<Entity>>();

        var (entityStream, mappings) = r.Value;

        var mappingsDict = mappings.ToDictionary(
            x => x.Name,
            x => GetStringList(x.Value),
            StringComparer.OrdinalIgnoreCase
        );

        var propertiesToRemove = mappingsDict
            .SelectMany(x => x.Value)
            .ToHashSet();

        var newEntityStream = entityStream
            .Select(e => ChangePropertyNames(e, mappingsDict, propertiesToRemove));

        return newEntityStream;

        static Entity ChangePropertyNames(
            Entity entity,
            IReadOnlyDictionary<string, IReadOnlyList<EntityPropertyKey>> mappings,
            IEnumerable<EntityPropertyKey> propertiesToRemove)
        {
            var changed = false;

            var newEntity = entity;

            foreach (var (newName, propertyKeys) in mappings)
            foreach (var entityPropertyKey in propertyKeys)
            {
                var value = entity.TryGetProperty(entityPropertyKey);

                if (value.HasValue)
                {
                    changed = true;
                    var newProperty = value.GetValueOrThrow();

                    newEntity = newEntity.WithProperty(
                        newName,
                        newProperty.Value,
                        newProperty.Order
                    );

                    break;
                }
            }

            var withoutProperties = newEntity.TryRemoveProperties(propertiesToRemove);

            if (withoutProperties.HasValue)
            {
                newEntity = withoutProperties.GetValueOrThrow();
                changed   = true;
            }

            if (!changed)
                return entity;

            return newEntity;
        }

        static IReadOnlyList<EntityPropertyKey> GetStringList(ISCLObject ev)
        {
            if (ev is ISCLObject.NestedList nestedList)
            {
                return nestedList.Value
                    .Select(x => EntityPropertyKey.Create(x.GetPrimitiveString()))
                    .ToList();
            }

            return new[] { EntityPropertyKey.Create(ev.GetPrimitiveString()) };
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
}
