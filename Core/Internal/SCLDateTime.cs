namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A datetime in the SCL type system
/// </summary>
public sealed record SCLDateTime(DateTime Value) : ISCLObject
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

        if (new StringStream(Serialize(SerializeOptions.Primitive)) is T vString)
            return vString;

        return Maybe<T>.None;
    }

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
}
