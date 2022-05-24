namespace Reductech.Sequence.Core.Entities.Schema;

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
        ISCLObject isclObject,
        TransformSettings transformSettings)
    {
        return Maybe<ISCLObject>.None;
    }

    /// <inheritdoc />
    public override bool IsSuperset(StringFormat other) => true;

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        //Do nothing
    }
}
