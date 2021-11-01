using System.Collections.Generic;
using System.Linq;
using Generator.Equals;
using Json.Schema;

namespace Reductech.EDR.Core.Entities.Schema
{

/// <summary>
/// Node data for enumerated values. Restricts the possible values of the node to one of several possible values.
/// </summary>
[Equatable]
public partial record EnumeratedValuesNodeData(
    [property: OrderedEquality] IReadOnlyList<EntityValue>? AllowedValues) : NodeData<
    EnumeratedValuesNodeData>
{
    /// <inheritdoc />
    public override EnumeratedValuesNodeData Combine(EnumeratedValuesNodeData other)
    {
        if (AllowedValues is null)
            return other;

        if (other.AllowedValues is null)
            return this;

        if (AllowedValues.SequenceEqual(other.AllowedValues))
            return this;

        return new EnumeratedValuesNodeData(
            AllowedValues.Concat(other.AllowedValues).Distinct().ToList()
        );
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        if (AllowedValues is null)
            return;

        var values = AllowedValues
            .Select(x => x)
            .ToArray();

        if (values.Length == 1)
            builder.Const(values.Single().ToJsonElement());
        else if (values.Length > 1)
            builder.Enum(values.Select(x => x.ToJsonElement()));
    }

    /// <summary>
    /// Whether a particular value is allowed
    /// </summary>
    public bool Allow(EntityValue entityValue, TransformSettings transformSettings)
    {
        if (AllowedValues is null)
            return true;

        return AllowedValues.Contains(entityValue);
    }

    /// <summary>
    /// Empty EnumeratedValuesNodeData
    /// </summary>
    public static EnumeratedValuesNodeData Empty { get; } = new(null as IReadOnlyList<EntityValue>);
}

}
