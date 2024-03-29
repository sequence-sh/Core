﻿namespace Sequence.Core.Entities.Schema;

/// <summary>
/// A node that can take one of a range of values
/// </summary>
public record OneOfNode(OneOfNodeData Data) : SchemaNode<OneOfNodeData>(
    EnumeratedValuesNodeData.Empty,
    Data
)
{
    /// <inheritdoc />
    public override Maybe<TypeReference> ToTypeReference()
    {
        var options =
            Data.Options.Select(x => x.ToTypeReference())
                .SelectMany(x => x.ToEnumerable())
                .Distinct()
                .ToArray();

        if (!options.Any())
            return Maybe<TypeReference>.None;

        return new TypeReference.OneOf(options);
    }

    /// <inheritdoc />
    public override IEnumerable<INodeData> NodeData
    {
        get
        {
            yield return Data;
        }
    }

    /// <inheritdoc />
    public override SchemaValueType SchemaValueType => SchemaValueType.Object;

    /// <inheritdoc />
    public override bool IsSuperset(SchemaNode other)
    {
        if (other is OneOfNode otherOneOf)
        {
            return Data.IsSuperset(otherOneOf.Data);
        }
        else
        {
            return Data.Options.Any(x => x.IsSuperset(other));
        }
    }

    /// <inheritdoc />
    protected override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
        string propertyName,
        ISCLObject value,
        TransformSettings transformSettings,
        TransformRoot transformRoot)
    {
        foreach (var possible in Data.Options)
        {
            var v = possible.TryTransform(propertyName, value, transformSettings, transformRoot);

            if (v.IsSuccess)
                return v;
        }

        return ErrorCode.SchemaViolated.ToErrorBuilder(
            "Does not match any option",
            propertyName,
            transformRoot.RowNumber,
            transformRoot.Entity
        );
    }
}

/// <inheritdoc />
public sealed record OneOfNodeData(IReadOnlyList<SchemaNode> Options) : NodeData<OneOfNodeData>
{
    /// <summary>
    /// Are the allowed values a superset (not strict) of the allowed values of the other node.
    /// </summary>
    public bool IsSuperset(OneOfNodeData other)
    {
        foreach (var option in other.Options)
        {
            var okay = Options.Any(x => x.IsSuperset(option));

            if (okay)
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override OneOfNodeData Combine(OneOfNodeData other)
    {
        return new(Options.Concat(other.Options).Distinct().ToList());
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        var schemas = Options.Select(
                x =>
                    x.ToJsonSchema()
            )
            .ToArray();

        builder.AnyOf(schemas);
    }
}
