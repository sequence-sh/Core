namespace Reductech.EDR.Core;

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
    public string Name => "Null";

    /// <inheritdoc />
    public string Serialize() => Name;

    /// <inheritdoc />
    public TypeReference TypeReference => TypeReference.Actual.Null;

    /// <inheritdoc />
    public object? ToCSharpObject()
    {
        return null;
    }

    /// <inheritdoc />
    public ISCLObject DefaultValue => Instance;

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
}
