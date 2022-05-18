namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A member of a set of predefined values
/// </summary>
public interface ISCLEnum : IComparableSCLObject
{
    /// <summary>
    /// The name of the enum type
    /// </summary>
    string TypeName { get; }

    /// <summary>
    /// The enum value
    /// </summary>
    string EnumValue { get; }
}

/// <summary>
/// A member of a set of predefined values
/// </summary>
public sealed record SCLEnum<T>(T Value) : ISCLEnum where T : struct, Enum
{
    /// <inheritdoc />
    public string Serialize(SerializeOptions _) => typeof(T).Name + "." + Value;

    /// <inheritdoc />
    public override string ToString() => Serialize(SerializeOptions.Name);

    /// <inheritdoc />
    public string TypeName => typeof(T).Name;

    /// <inheritdoc />
    public string EnumValue => Value.ToString();

    /// <inheritdoc />
    public TypeReference GetTypeReference() => new TypeReference.Enum(typeof(T));

    /// <inheritdoc />
    public int CompareTo(object? obj) => obj switch
    {
        null                 => 0.CompareTo(null),
        SCLEnum<T> otherEnum => Value.CompareTo(otherEnum.Value),
        ISCLObject other => StringComparer.Ordinal.Compare(
            Serialize(SerializeOptions.Primitive),
            other.Serialize(SerializeOptions.Primitive)
        ),
        _ => 0.CompareTo(null),
    };

    /// <inheritdoc />
    public Maybe<T1> MaybeAs<T1>() where T1 : ISCLObject
    {
        if (this is T1 vEnum)
            return vEnum;

        return Maybe<T1>.None;
    }

    /// <inheritdoc />
    public object ToCSharpObject() => Value;

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(
        string path,
        SchemaConversionOptions? schemaConversionOptions)
    {
        return StringNode.Default;
    }

    /// <summary>
    /// Implicit operator
    /// </summary>
    public static implicit operator T(SCLEnum<T> sclEnum) => sclEnum.Value;

    /// <summary>
    /// Explicit operator
    /// </summary>
    public static explicit operator SCLEnum<T>(T @enum) => new(@enum);

    /// <inheritdoc />
    public IConstantFreezableStep ToConstantFreezableStep(TextLocation location) =>
        new SCLConstantFreezable<SCLEnum<T>>(this, location);

    /// <inheritdoc />
    public bool IsEmpty() => false;
}
