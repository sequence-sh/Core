namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A discriminated union with two possibilities
/// </summary>
public readonly record struct SCLOneOf<T0, T1>(OneOf<T0, T1> OneOf) : ISCLOneOf
    where T0 : ISCLObject where T1 : ISCLObject
{
    /// <inheritdoc />
    public string Name => OneOf.Match(x => x.Name, x => x.Name);

    /// <inheritdoc />
    public TypeReference TypeReference => new TypeReference.OneOf(
        new[] { typeof(T0), typeof(T1) }.Select(TypeReference.Create).ToArray()
    );

    /// <inheritdoc />
    public string Serialize() => Value.Serialize();

    /// <inheritdoc />
    public object? ToCSharpObject() => Value.ToCSharpObject();

    /// <inheritdoc />
    public ISCLObject Value => OneOf.Match(x => x, x => x as ISCLObject);

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject => Value.MaybeAs<T>();

    /// <inheritdoc />
    public ISCLObject DefaultValue =>
        new SCLOneOf<T0, T1>(OneOf<T0, T1>.FromT0(ISCLObject.GetDefaultValue<T0>()));

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(string path, SchemaConversionOptions? schemaConversionOptions) =>
        Value.ToSchemaNode(path, schemaConversionOptions);
}

/// <summary>
/// A discriminated union with three possibilities
/// </summary>
public readonly record struct SCLOneOf<T0, T1, T2>(OneOf<T0, T1, T2> OneOf) : ISCLOneOf
    where T0 : ISCLObject where T1 : ISCLObject where T2 : ISCLObject
{
    /// <inheritdoc />
    public string Name => OneOf.Match(x => x.Name, x => x.Name, x => x.Name);

    /// <inheritdoc />
    public TypeReference TypeReference => new TypeReference.OneOf(
        new[] { typeof(T0), typeof(T1), typeof(T2) }.Select(TypeReference.Create).ToArray()
    );

    /// <inheritdoc />
    public string Serialize() => Value.Serialize();

    /// <inheritdoc />
    public object? ToCSharpObject() => Value.ToCSharpObject();

    /// <inheritdoc />
    public ISCLObject Value => OneOf.Match(x => x, x => x, x => x as ISCLObject);

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject => Value.MaybeAs<T>();

    /// <inheritdoc />
    public ISCLObject DefaultValue =>
        new SCLOneOf<T0, T1, T2>(OneOf<T0, T1, T2>.FromT0(ISCLObject.GetDefaultValue<T0>()));

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(string path, SchemaConversionOptions? schemaConversionOptions) =>
        Value.ToSchemaNode(path, schemaConversionOptions);
}
