﻿using Generator.Equals;

namespace Sequence.Core.Entities.Schema;

/// <summary>
/// Node data for enumerated values. Restricts the possible values of the node to one of several possible values.
/// </summary>
[Equatable]
public partial record EnumeratedValuesNodeData(
    [property: OrderedEquality] IReadOnlyList<ISCLObject>? AllowedValues) : NodeData<
    EnumeratedValuesNodeData>
{
    /// <summary>
    /// Are the allowed values a superset (not strict) of the allowed values of the other node.
    /// </summary>
    public bool IsSuperset(EnumeratedValuesNodeData other)
    {
        if (AllowedValues is null)
            return true;

        if (other.AllowedValues is null)
            return false;

        if (other.AllowedValues.Count > AllowedValues.Count)
            return false;

        foreach (var otherAllowedValue in other.AllowedValues)
        {
            if (!AllowedValues.Contains(otherAllowedValue))
                return false;
        }

        return true;
    }

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
            builder.Const(values.Single().ToJsonElement().AsNode());
        else if (values.Length > 1)
            builder.Enum(values.Select(x => x.ToJsonElement().AsNode()));
    }

    /// <summary>
    /// Whether a particular value is allowed
    /// </summary>
    public bool Allow(ISCLObject entityValue, TransformSettings transformSettings)
    {
        if (AllowedValues is null)
            return true;

        return AllowedValues.Contains(entityValue);
    }

    /// <summary>
    /// Empty EnumeratedValuesNodeData
    /// </summary>
    public static EnumeratedValuesNodeData Empty { get; } = new(null as IReadOnlyList<ISCLObject>);
}
