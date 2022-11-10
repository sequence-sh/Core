namespace Sequence.Core.Internal;

/// <summary>
/// A datetime in the SCL type system
/// </summary>
public sealed record SCLDateTime(DateTime Value) : IComparableSCLObject
{
    /// <inheritdoc />
    public string Serialize(SerializeOptions _) => Value.ToString(Constants.DateTimeFormat);

    /// <inheritdoc />
    public TypeReference GetTypeReference() => TypeReference.Actual.Date;

    /// <inheritdoc />
    public object ToCSharpObject() => Value;

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject
    {
        if (this is T vBool)
            return vBool;

        return Maybe<T>.None;
    }

    /// <inheritdoc />
    public int CompareTo(object? obj) => obj switch
    {
        null                    => Value.CompareTo(null),
        SCLDateTime sclDateTime => Value.CompareTo(sclDateTime.Value),
        ISCLObject other => StringComparer.Ordinal.Compare(
            Serialize(SerializeOptions.Primitive),
            other.Serialize(SerializeOptions.Primitive)
        ),
        _ => Value.CompareTo(null)
    };

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(
        string path,
        SchemaConversionOptions? schemaConversionOptions)
    {
        return new StringNode(
            EnumeratedValuesNodeData.Empty,
            DateTimeStringFormat.Instance,
            StringRestrictions.NoRestrictions
        );
    }

    /// <summary>
    /// Implicit operator
    /// </summary>
    public static implicit operator DateTime(SCLDateTime sclDateTime) => sclDateTime.Value;

    /// <summary>
    /// Explicit operator
    /// </summary>
    public static explicit operator SCLDateTime(DateTime dt) => new(dt);

    /// <inheritdoc />
    public IConstantFreezableStep ToConstantFreezableStep(TextLocation location) =>
        new SCLConstantFreezable<SCLDateTime>(this, location);

    /// <inheritdoc />
    public bool IsEmpty() => false;
}
