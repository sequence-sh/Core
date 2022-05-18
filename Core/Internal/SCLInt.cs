namespace Reductech.Sequence.Core.Internal;

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
    public int CompareTo(object? obj) => obj switch
    {
        null                => Value.CompareTo(null),
        SCLDouble sclDouble => Value.CompareTo(sclDouble.Value),
        SCLInt sclInt       => Value.CompareTo(sclInt.Value),
        ISCLObject other => StringComparer.Ordinal.Compare(
            Serialize(SerializeOptions.Primitive),
            other.Serialize(SerializeOptions.Primitive)
        ),
        _ => Value.CompareTo(null)
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

    /// <inheritdoc />
    public IConstantFreezableStep ToConstantFreezableStep(TextLocation location) =>
        new SCLConstantFreezable<SCLInt>(this, location);

    /// <inheritdoc />
    public bool IsEmpty() => false;
}
