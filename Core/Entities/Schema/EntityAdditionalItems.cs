namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// A schema which, if matched, allows additional items
/// </summary>
public record EntityAdditionalItems(SchemaNode AdditionalItems) : NodeData<EntityAdditionalItems>
{
    /// <inheritdoc />
    public override EntityAdditionalItems Combine(EntityAdditionalItems other)
    {
        return new EntityAdditionalItems(AdditionalItems.Combine(other.AdditionalItems));
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        builder.AdditionalItems(AdditionalItems.ToJsonSchema());
    }
}
