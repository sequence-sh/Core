namespace Reductech.Sequence.Core;

/// <summary>
/// The Null value in SCL
/// </summary>
public record SCLNull : ISCLObject
{
    private SCLNull() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static SCLNull Instance { get; } = new();

    /// <inheritdoc />
    public string Serialize(SerializeOptions _) => "Null";

    /// <inheritdoc />
    public TypeReference GetTypeReference() => TypeReference.Actual.Null;

    /// <inheritdoc />
    public object? ToCSharpObject()
    {
        return null;
    }

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject
    {
        if (this is T value)
            return value;

        return Maybe<T>.None;
    }

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(
        string path,
        SchemaConversionOptions? schemaConversionOptions) => NullNode.Instance;

    /// <inheritdoc />
    JsonElement ISCLObject.ToJsonElement() => JsonDocument.Parse("null").RootElement;

    /// <inheritdoc />
    public IConstantFreezableStep ToConstantFreezableStep(TextLocation location) =>
        new NullConstant(location);
}
