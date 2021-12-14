namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A double in the SCL type system
/// </summary>
public readonly record struct SCLDouble(double Value) : IComparableSCLObject
{
    /// <inheritdoc />
    public string Serialize() => Value.ToString(Constants.DoubleFormat);

    /// <inheritdoc />
    public string Name => Value.ToString(Constants.DoubleFormat);

    /// <inheritdoc />
    public TypeReference TypeReference => TypeReference.Actual.Double;

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
        if (this is T vDouble)
            return vDouble;

        if (new StringStream(Serialize()) is T vString)
            return vString;

        return Maybe<T>.None;
    }

    /// <inheritdoc />
    public ISCLObject DefaultValue => new SCLDouble(0);

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(
        string path,
        SchemaConversionOptions? schemaConversionOptions) => NumberNode.Default;

    /// <summary>
    /// Implicit operator
    /// </summary>
    public static implicit operator double(SCLDouble sclDouble) => sclDouble.Value;

    /// <summary>
    /// Explicit operator
    /// </summary>
    public static explicit operator SCLDouble(double i) => new(i);
}
