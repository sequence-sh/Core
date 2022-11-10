using Generator.Equals;

namespace Sequence.Core.Entities.Schema;

/// <summary>
/// Data about entity properties
/// </summary>
[Equatable]
public partial record EntityPropertiesData(
    [property: SetEquality]
    IReadOnlyDictionary<string, (SchemaNode Node, bool Required, int Order)> Nodes) :
    NodeData<
        EntityPropertiesData>
{
    /// <summary>
    /// Are the allowed values a superset (not strict) of the allowed values of the other node.
    /// </summary>
    public bool IsSuperset(EntityPropertiesData other, SchemaNode allowExtra)
    {
        /*This is a superset if:
            Every required property is also required by other
            Every property of other is either present and a subset or matched by allow extra
        */

        var remainingRequired = this.Nodes.Where(x => x.Value.Required)
            .Select(x => x.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, (otherNode, otherRequired, _)) in other.Nodes)
        {
            if (Nodes.TryGetValue(key, out var m))
            {
                if (!m.Node.IsSuperset(otherNode))
                {
                    if (!allowExtra.IsSuperset(otherNode))
                        return false;
                }

                if (otherRequired)
                    remainingRequired.Remove(key);
            }
            else if (!allowExtra.IsSuperset(otherNode))
            {
                return false;
            }
        }

        if (remainingRequired.Any())
            return false;

        return true;
    }

    /// <inheritdoc />
    public override EntityPropertiesData Combine(EntityPropertiesData other)
    {
        if (this == Empty)
            return other;

        if (other == Empty)
            return this;

        var newDict = new Dictionary<string, (SchemaNode Node, bool Required, int Order)>();
        var order   = 0;

        foreach (var group in Nodes.Concat(other.Nodes)
                     .GroupBy(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase))
        {
            var key = group.Key;

            if (group.Count() == 1)
                newDict[key] = (group.Single().Node, false, order);

            else if (group.Count() == 2)
            {
                var (first, second) = group.GetFirstTwo().GetValueOrThrow();

                newDict[key] = (first.Node.Combine(second.Node), first.Required && second.Required,
                                order);
            }
            else
                throw new NotImplementedException("Group should have either one or two members");

            order++;
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
        ImmutableDictionary<string, (SchemaNode Node, bool Required, int Order)>.Empty
    );
}
