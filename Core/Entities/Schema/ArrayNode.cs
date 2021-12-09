using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Json.Schema;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Entities.Schema;

/// <summary>
/// Schema is matched by an array
/// </summary>
public record ArrayNode(
        EnumeratedValuesNodeData EnumeratedValuesNodeData,
        ItemsData ItemsData)
    : SchemaNode<ItemsData>(
        EnumeratedValuesNodeData,
        ItemsData
    )
{
    /// <inheritdoc />
    public override SchemaValueType SchemaValueType => SchemaValueType.Array;

    /// <inheritdoc />
    public override bool IsMorePermissive(SchemaNode other)
    {
        return false;
    }

    /// <inheritdoc />
    protected override Result<Maybe<EntityValue>, IErrorBuilder> TryTransform1(
        string propertyName,
        EntityValue entityValue,
        TransformSettings transformSettings)
    {
        ImmutableList<EntityValue> immutableList;
        bool                       changed;

        if (entityValue is EntityValue.NestedList nestedList)
        {
            immutableList = nestedList.Value;
            changed       = false;
        }
        else
        {
            var delimiters = transformSettings.MultiValueFormatter.GetFormats(propertyName)
                .ToArray();

            if (delimiters.Any())
                immutableList = entityValue.GetPrimitiveString()
                    .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => new EntityValue.String(x) as EntityValue)
                    .ToImmutableList();
            else
                immutableList = ImmutableList<EntityValue>.Empty;

            changed = true;
        }

        var newList = immutableList;
        var errors  = new List<IErrorBuilder>();

        for (var i = 0; i < immutableList.Count; i++)
        {
            var ev = immutableList[i];

            var schemaNode = i < ItemsData.PrefixItems.Count
                ? ItemsData.PrefixItems[i]
                : ItemsData.AdditionalItems;

            var transformResult = schemaNode.TryTransform(
                $"{propertyName}[{i}]",
                ev,
                transformSettings
            );

            if (transformResult.IsFailure)
                errors.Add(transformResult.Error);
            else if (transformResult.Value.HasValue)
            {
                changed = true;
                newList = newList.SetItem(i, transformResult.Value.GetValueOrThrow());
            }
            else
            {
                //do nothing
            }
        }

        if (errors.Any())
            return Result.Failure<Maybe<EntityValue>, IErrorBuilder>(
                ErrorBuilderList.Combine(errors)
            );

        if (!changed)
            return Maybe<EntityValue>.None;

        return Maybe<EntityValue>.From(new EntityValue.NestedList(newList));
    }
}
