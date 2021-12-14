namespace Reductech.EDR.Core.Internal;

/// <summary>
/// An integer in the SCL type system
/// </summary>
public readonly record struct SCLInt(int Value) : IComparableSCLObject
{
    /// <inheritdoc />
    public string Serialize(SerializeOptions _) => Value.ToString();

    /// <inheritdoc />
    public TypeReference GetTypeReference() => TypeReference.Actual.Integer;

    /// <inheritdoc />
    public int CompareTo(IComparableSCLObject? other) => other switch
    {
        null                => Value.CompareTo(null),
        SCLDouble sclDouble => Value.CompareTo(sclDouble.Value),
        SCLInt sclInt       => Value.CompareTo(sclInt.Value),
        _ => StringComparer.Ordinal.Compare(
            Serialize(SerializeOptions.Primitive),
            other.Serialize(SerializeOptions.Primitive)
        )
    };

    /// <inheritdoc />
    public object ToCSharpObject() => Value;

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject
    {
        if (this is T vInt)
            return vInt;

        if (new SCLDouble(Value) is T vDouble)
            return vDouble;

        if (new StringStream(Serialize(SerializeOptions.Primitive)) is T vString)
            return vString;

        return Maybe<T>.None;
    }

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(
        string path,
        SchemaConversionOptions? schemaConversionOptions)
    {
        return IntegerNode.Default;
    }

    /// <summary>
    /// Implicit operator
    /// </summary>
    public static implicit operator int(SCLInt sclInt) => sclInt.Value;

    /// <summary>
    /// Explicit operator
    /// </summary>
    public static explicit operator SCLInt(int i) => new(i);
}
