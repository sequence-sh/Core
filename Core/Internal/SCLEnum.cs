namespace Reductech.EDR.Core.Internal;

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
    public string Name => typeof(T).Name + "." + Value;

    /// <inheritdoc />
    public string Serialize() => Name;

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <inheritdoc />
    public string TypeName => typeof(T).Name;

    /// <inheritdoc />
    public string EnumValue => Value.ToString();

    /// <inheritdoc />
    public TypeReference TypeReference => new TypeReference.Enum(typeof(T));

    /// <inheritdoc />
    public int CompareTo(IComparableSCLObject? other) => other switch
    {
        null                 => 0.CompareTo(null),
        SCLEnum<T> otherEnum => Value.CompareTo(otherEnum.Value),
        _                    => StringComparer.Ordinal.Compare(Serialize(), other.Serialize())
    };

    /// <inheritdoc />
    public Maybe<T1> MaybeAs<T1>() where T1 : ISCLObject
    {
        if (this is T1 vEnum)
            return vEnum;

        if (new StringStream(Serialize()) is T1 vString)
            return vString;

        return Maybe<T1>.None;
    }

    /// <inheritdoc />
    public ISCLObject DefaultValue => new SCLEnum<T>(default(T));

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
}
