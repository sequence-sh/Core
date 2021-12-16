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
    public override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform(
        string propertyName,
        ISCLObject ISCLObject,
        TransformSettings transformSettings)
    {
        return Maybe<ISCLObject>.None;
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        //Do nothing
    }
}
