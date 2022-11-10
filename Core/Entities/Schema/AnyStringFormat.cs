namespace Sequence.Core.Entities.Schema;

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
        TransformSettings transformSettings,
        TransformRoot transformRoot)
    {
        return Maybe<ISCLObject>.None;
    }

    /// <inheritdoc />
    public override bool IsSuperset(StringFormat other) => true;

    /// <inheritdoc />
    public override TypeReference GetTypeReference(StringRestrictions restrictions)
    {
        if (restrictions == StringRestrictions.NoRestrictions)
            return TypeReference.Any.Instance;

        return TypeReference.Actual.String;
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        //Do nothing
    }
}
