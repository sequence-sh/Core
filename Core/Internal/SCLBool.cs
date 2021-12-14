namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A boolean in the SCL type system
/// </summary>
public sealed record SCLBool : ISCLObject
{
    private SCLBool() { }

    /// <summary>
    /// The True value
    /// </summary>
    public static SCLBool True { get; } = new() { Value = true };

    /// <summary>
    /// The False value
    /// </summary>
    public static SCLBool False { get; } = new() { Value = false };

    /// <summary>
    /// Create an SCLBool from a bool
    /// </summary>
    public static SCLBool Create(bool b) => b ? True : False;

    /// <summary>
    /// The value of this Boolean
    /// </summary>
    public bool Value { get; private init; }

    /// <inheritdoc />
    public string Serialize() => Value.ToString();

    /// <inheritdoc />
    public string Name => Value.ToString();

    /// <inheritdoc />
    public TypeReference TypeReference => TypeReference.Actual.Bool;

    /// <inheritdoc />
    public object ToCSharpObject() => Value;

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject
    {
        if (this is T vBool)
            return vBool;

        if (new StringStream(Serialize()) is T vString)
            return vString;

        return Maybe<T>.None;
    }

    /// <inheritdoc />
    public ISCLObject DefaultValue => False;

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(
        string path,
        SchemaConversionOptions? schemaConversionOptions) => BooleanNode.Default;

    /// <summary>
    /// Implicit operator
    /// </summary>
    public static implicit operator bool(SCLBool sclBool) => sclBool.Value;

    /// <summary>
    /// Explicit operator
    /// </summary>
    public static explicit operator SCLBool(bool b) => Create(b);
}
