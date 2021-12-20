namespace Reductech.EDR.Core.Util;

/// <summary>
/// The result of a step with not return value.
/// </summary>
public sealed record Unit : ISCLObject
{
    /// <summary>
    /// The Unit.
    /// </summary>
    public static readonly Unit Default = new();

    private Unit() { }

    /// <inheritdoc />
    public string Serialize(SerializeOptions _) => "Unit";

    /// <inheritdoc />
    public TypeReference GetTypeReference() => TypeReference.Unit.Instance;

    /// <inheritdoc />
    public object ToCSharpObject() => Default;

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject
    {
        if (this is T value)
            return value;

        return Maybe<T>.None;
    }

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(string path, SchemaConversionOptions? schemaConversionOptions)
    {
        return new NullNode();
        //Not really sure what to do here
    }
}
