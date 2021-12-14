namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A discriminated union with two possibilities
/// </summary>
public readonly record struct SCLOneOf<T0, T1>(OneOf<T0, T1> OneOf) : ISCLOneOf
    where T0 : ISCLObject where T1 : ISCLObject
{
    /// <inheritdoc />
    public TypeReference GetTypeReference() => new TypeReference.OneOf(
        new[] { typeof(T0), typeof(T1) }.Select(TypeReference.Create).ToArray()
    );

    /// <inheritdoc />
    public string Serialize(SerializeOptions options) => Value.Serialize(options);

    /// <inheritdoc />
    public object? ToCSharpObject() => Value.ToCSharpObject();

    /// <inheritdoc />
    public ISCLObject Value => OneOf.Match(x => x, x => x as ISCLObject);

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject => Value.MaybeAs<T>();

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(string path, SchemaConversionOptions? schemaConversionOptions) =>
        Value.ToSchemaNode(path, schemaConversionOptions);

    /// <summary>
    /// Get the default value
    /// </summary>
    /// <returns></returns>
    public static SCLOneOf<T0, T1> GetDefaultValue() => new(ISCLObject.GetDefaultValue<T0>());
}

/// <summary>
/// A discriminated union with three possibilities
/// </summary>
public readonly record struct SCLOneOf<T0, T1, T2>(OneOf<T0, T1, T2> OneOf) : ISCLOneOf
    where T0 : ISCLObject where T1 : ISCLObject where T2 : ISCLObject
{
    /// <inheritdoc />
    public TypeReference GetTypeReference() => new TypeReference.OneOf(
        new[] { typeof(T0), typeof(T1), typeof(T2) }.Select(TypeReference.Create).ToArray()
    );

    /// <inheritdoc />
    public string Serialize(SerializeOptions options) => Value.Serialize(options);

    /// <inheritdoc />
    public object? ToCSharpObject() => Value.ToCSharpObject();

    /// <inheritdoc />
    public ISCLObject Value => OneOf.Match(x => x, x => x, x => x as ISCLObject);

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject => Value.MaybeAs<T>();

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(string path, SchemaConversionOptions? schemaConversionOptions) =>
        Value.ToSchemaNode(path, schemaConversionOptions);

    /// <summary>
    /// Get the default value
    /// </summary>
    /// <returns></returns>
    public static SCLOneOf<T0, T1, T2> GetDefaultValue() => new(ISCLObject.GetDefaultValue<T0>());
}
