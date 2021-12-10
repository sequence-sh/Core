using Generator.Equals;

namespace Reductech.EDR.Core.Entities.Schema;

/// <summary>
/// Data about entity properties
/// </summary>
[Equatable]
public partial record EntityPropertiesData(
    [property: SetEquality] IReadOnlyDictionary<string, (SchemaNode Node, bool Required)> Nodes) :
    NodeData<
        EntityPropertiesData>
{
    /// <inheritdoc />
    public override EntityPropertiesData Combine(EntityPropertiesData other)
    {
        if (this == Empty)
            return other;

        if (other == Empty)
            return this;

        var newDict = new Dictionary<string, (SchemaNode Node, bool Required)>();

        foreach (var group in Nodes.Concat(other.Nodes)
                     .GroupBy(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase))
        {
            var key = group.Key;

            if (group.Count() == 1)
                newDict[key] = (group.Single().Node, false);

            else if (group.Count() == 2)
            {
                var (first, second) = group.GetFirstTwo().GetValueOrThrow();

                newDict[key] = (first.Node.Combine(second.Node), first.Required && second.Required);
            }
        }

        return new EntityPropertiesData(newDict);
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        var requiredStuff = Nodes
            .Where(x => x.Value.Required)
            .Select(x => x.Key)
            .ToList();

        if (requiredStuff.Any())
            builder.Required(requiredStuff);

        var props = Nodes.ToDictionary(x => x.Key, x => x.Value.Node.ToJsonSchema());

        if (props.Any())
            builder.Properties(props);
    }

    /// <summary>
    /// EntityPropertiesData with no Properties
    /// </summary>
    public static EntityPropertiesData Empty { get; } = new(
        ImmutableDictionary<string, (SchemaNode Node, bool Required)>.Empty
    );
}
