using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Generator.Equals;
using Json.Schema;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Entities.Schema;

/// <summary>
/// List of prefix items
/// </summary>
[Equatable]
public partial record ItemsData(
    [property: OrderedEquality] IReadOnlyList<SchemaNode> PrefixItems,
    SchemaNode AdditionalItems) : NodeData<ItemsData>
{
    /// <inheritdoc />
    public override ItemsData Combine(ItemsData other)
    {
        var combinedAdditionalItems = AdditionalItems.Combine(other.AdditionalItems);

        Result<IReadOnlyList<SchemaNode>, IErrorBuilder> prefixResult;

        if (PrefixItems.Count >= other.PrefixItems.Count)
        {
            var l = CombineLists(PrefixItems, other.PrefixItems, AdditionalItems);
            prefixResult = l;
        }
        else
        {
            var l = CombineLists(other.PrefixItems, PrefixItems, other.AdditionalItems);
            prefixResult = l;
        }

        return new ItemsData(prefixResult.Value, combinedAdditionalItems);

        static Result<IReadOnlyList<SchemaNode>, IErrorBuilder> CombineLists(
            IReadOnlyList<SchemaNode> longList,
            IReadOnlyList<SchemaNode> shortList,
            SchemaNode defaultNode)
        {
            List<SchemaNode> newList = new();

            for (var index = 0; index < longList.Count; index++)
            {
                var        longNode = longList[index];
                SchemaNode shortNode;

                if (index < shortList.Count)
                    shortNode = shortList[index];
                else
                    shortNode = defaultNode;

                var combineResult = longNode.Combine(shortNode);

                newList.Add(combineResult);
            }

            return Result.Success<IReadOnlyList<SchemaNode>, IErrorBuilder>(newList);
        }
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        var schema = AdditionalItems.ToJsonSchema();
        builder.Items(schema);

        var schemas = PrefixItems.Select(x => x.ToJsonSchema()).ToList();
        builder.PrefixItems(schemas);
    }
}
