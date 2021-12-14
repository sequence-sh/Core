namespace Reductech.EDR.Core.Internal;

/// <summary>
/// An integer in the SCL type system
/// </summary>
public readonly record struct SCLInt(int Value) : IComparableSCLObject
{
    /// <inheritdoc />
    public string Serialize() => Value.ToString();

    /// <inheritdoc />
    public TypeReference TypeReference => TypeReference.Actual.Integer;

    /// <inheritdoc />
    public string Name => Value.ToString();

    /// <inheritdoc />
    public int CompareTo(IComparableSCLObject? other) => other switch
    {
        null                => Value.CompareTo(null),
        SCLDouble sclDouble => Value.CompareTo(sclDouble.Value),
        SCLInt sclInt       => Value.CompareTo(sclInt.Value),
        _                   => StringComparer.Ordinal.Compare(Serialize(), other.Serialize())
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

        if (new StringStream(Serialize()) is T vString)
            return vString;

        return Maybe<T>.None;
    }

    /// <inheritdoc />
    public ISCLObject DefaultValue => new SCLInt(0);

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
