namespace Reductech.EDR.Core.Internal;

///// <summary>
///// A member of a set of predefined values
///// </summary>
//public sealed record Enumeration(string Type, string Value) : ISCLObject
//{
//    /// <inheritdoc />
//    public override string ToString() => Name;

//    /// <summary>
//    /// Try to convert this to a C# enum
//    /// </summary>
//    public Maybe<T> TryConvert<T>() where T : struct, Enum
//    {
//        if (Enum.TryParse(Value, true, out T t))
//            return t;

//        return Maybe<T>.None;
//    }

//    /// <inheritdoc />
//    public string Name => Type + "." + Value;

//    /// <inheritdoc />
//    string ISCLObject.Serialize() => Name;
//}

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
public sealed record SCLEnum<T>(T Value) : ISCLEnum where T : Enum
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
}
