namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A discriminated union with two possibilities
/// </summary>
public readonly record struct SCLOneOf<T0, T1>(OneOf<T0, T1> OneOf) : ISCLOneOf
    where T0 : ISCLObject where T1 : ISCLObject
{
    /// <summary>
    /// Create from an SCLObject
    /// </summary>
    public static Maybe<SCLOneOf<T0, T1>> TryCreate(string propertyName, ISCLObject obj)
    {
        var t0 = obj.TryConvertTyped<T0>(propertyName);

        if (t0.IsSuccess)
            return new SCLOneOf<T0, T1>(t0.Value);

        var t1 = obj.TryConvertTyped<T1>(propertyName);

        if (t1.IsSuccess)
            return new SCLOneOf<T0, T1>(t1.Value);

        return Maybe<SCLOneOf<T0, T1>>.None;
    }

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

    /// <summary>
    /// Create from an SCLObject
    /// </summary>
    public static Maybe<SCLOneOf<T0, T1, T2>> TryCreate(string propertyName, ISCLObject obj)
    {
        var t0 = obj.TryConvertTyped<T0>(propertyName);

        if (t0.IsSuccess)
            return new SCLOneOf<T0, T1, T2>(t0.Value);

        var t1 = obj.TryConvertTyped<T1>(propertyName);

        if (t1.IsSuccess)
            return new SCLOneOf<T0, T1, T2>(t1.Value);

        var t2 = obj.TryConvertTyped<T2>(propertyName);

        if (t2.IsSuccess)
            return new SCLOneOf<T0, T1, T2>(t2.Value);

        return Maybe<SCLOneOf<T0, T1, T2>>.None;
    }

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

/// <summary>
/// A discriminated union with three possibilities
/// </summary>
public readonly record struct SCLOneOf<T0, T1, T2, T3>(OneOf<T0, T1, T2, T3> OneOf) : ISCLOneOf
    where T0 : ISCLObject where T1 : ISCLObject where T2 : ISCLObject where T3 : ISCLObject
{
    /// <summary>
    /// Create from an SCLObject
    /// </summary>
    public static Maybe<SCLOneOf<T0, T1, T2, T3>> TryCreate(string propertyName, ISCLObject obj)
    {
        var t0 = obj.TryConvertTyped<T0>(propertyName);

        if (t0.IsSuccess)
            return new SCLOneOf<T0, T1, T2, T3>(t0.Value);

        var t1 = obj.TryConvertTyped<T1>(propertyName);

        if (t1.IsSuccess)
            return new SCLOneOf<T0, T1, T2, T3>(t1.Value);

        var t2 = obj.TryConvertTyped<T2>(propertyName);

        if (t2.IsSuccess)
            return new SCLOneOf<T0, T1, T2, T3>(t2.Value);

        var t3 = obj.TryConvertTyped<T3>(propertyName);

        if (t3.IsSuccess)
            return new SCLOneOf<T0, T1, T2, T3>(t3.Value);

        return Maybe<SCLOneOf<T0, T1, T2, T3>>.None;
    }

    /// <inheritdoc />
    public TypeReference GetTypeReference() => new TypeReference.OneOf(
        new[] { typeof(T0), typeof(T1), typeof(T2), typeof(T3) }.Select(TypeReference.Create)
            .ToArray()
    );

    /// <inheritdoc />
    public string Serialize(SerializeOptions options) => Value.Serialize(options);

    /// <inheritdoc />
    public object? ToCSharpObject() => Value.ToCSharpObject();

    /// <inheritdoc />
    public ISCLObject Value => OneOf.Match(x => x, x => x, x => x, x => x as ISCLObject);

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject => Value.MaybeAs<T>();

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(string path, SchemaConversionOptions? schemaConversionOptions) =>
        Value.ToSchemaNode(path, schemaConversionOptions);

    /// <summary>
    /// Get the default value
    /// </summary>
    public static SCLOneOf<T0, T1, T2, T3> GetDefaultValue() =>
        new(ISCLObject.GetDefaultValue<T0>());
}
