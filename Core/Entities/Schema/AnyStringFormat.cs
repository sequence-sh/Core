namespace Reductech.EDR.Core.Entities.Schema;

/// <summary>
/// Matches any string
/// </summary>
public record AnyStringFormat : StringFormat
{
    private AnyStringFormat() { }

    /// <summary>
    /// The Instance
    /// </summary>
    public static AnyStringFormat Instance { get; } = new();

    /// <inheritdoc />
    public override Result<Maybe<EntityValue>, IErrorBuilder> TryTransform(
        string propertyName,
        EntityValue entityValue,
        TransformSettings transformSettings)
    {
        return Maybe<EntityValue>.None;
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        //Do nothing
    }
}
