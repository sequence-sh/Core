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
    protected override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
        string propertyName,
        ISCLObject value,
        TransformSettings transformSettings)
    {
        ImmutableList<ISCLObject> immutableList;
        bool                      changed;

        if (value is IArray nestedList)
        {
            immutableList = nestedList.ListIfEvaluated().Value.ToImmutableList();
            changed       = false;
        }
        else
        {
            var delimiters = transformSettings.MultiValueFormatter.GetFormats(propertyName)
                .ToArray();

            if (delimiters.Any())
                immutableList = value.Serialize(SerializeOptions.Primitive)
                    .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => (ISCLObject)new StringStream(x))
                    .ToImmutableList();
            else
                immutableList = ImmutableList<ISCLObject>.Empty;

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
            return Result.Failure<Maybe<ISCLObject>, IErrorBuilder>(
                ErrorBuilderList.Combine(errors)
            );

        if (!changed)
            return Maybe<ISCLObject>.None;

        return Maybe<ISCLObject>.From(new EagerArray<ISCLObject>(newList));
    }
}
