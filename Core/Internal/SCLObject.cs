namespace Reductech.EDR.Core.Internal;

/// <summary>
/// Base class for all SCL Objects
/// </summary>
public interface ISCLObject
{
    /// <summary>
    /// Short name for this object
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Serialize this object
    /// </summary>
    string Serialize();

    /// <summary>
    /// The Type Reference
    /// </summary>
    TypeReference TypeReference { get; }
}

/// <summary>
/// A comparable SCL object
/// </summary>
public interface IComparableSCLObject : ISCLObject, IComparable<IComparableSCLObject> { }

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
}

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
}

/// <summary>
/// A discriminated union with two possibilities
/// </summary>
public readonly record struct SCLOneOf<T0, T1>(OneOf<T0, T1> OneOf) : ISCLObject
    where T0 : ISCLObject where T1 : ISCLObject
{
    /// <inheritdoc />
    public string Name => OneOf.Match(x => x.Name, x => x.Name);

    /// <inheritdoc />
    public string Serialize() => OneOf.Match(x => x.Serialize(), x => x.Serialize());

    /// <inheritdoc />
    public TypeReference TypeReference => new TypeReference.OneOf(
        new[] { typeof(T0), typeof(T1) }.Select(TypeReference.Create).ToArray()
    );
}

/// <summary>
/// A discriminated union with three possibilities
/// </summary>
public readonly record struct SCLOneOf<T0, T1, T2>(OneOf<T0, T1, T2> OneOf) : ISCLObject
    where T0 : ISCLObject where T1 : ISCLObject where T2 : ISCLObject
{
    /// <inheritdoc />
    public string Name => OneOf.Match(x => x.Name, x => x.Name, x => x.Name);

    /// <inheritdoc />
    public string Serialize() => OneOf.Match(
        x => x.Serialize(),
        x => x.Serialize(),
        x => x.Serialize()
    );

    /// <inheritdoc />
    public TypeReference TypeReference => new TypeReference.OneOf(
        new[] { typeof(T0), typeof(T1), typeof(T2) }.Select(TypeReference.Create).ToArray()
    );
}

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
    /// The value of this Boolean
    /// </summary>
    public bool Value { get; private init; }

    /// <inheritdoc />
    public string Serialize() => Value.ToString();

    /// <inheritdoc />
    public string Name => Value.ToString();

    /// <inheritdoc />
    public TypeReference TypeReference => TypeReference.Actual.Bool;
}

/// <summary>
/// A datetime in the SCL type system
/// </summary>
public sealed record SCLDateTime(DateTime Value) : ISCLObject
{
    /// <inheritdoc />
    public string Name => Value.ToString(Constants.DateTimeFormat);

    /// <inheritdoc />
    public string Serialize() => Value.ToString(Constants.DateTimeFormat);

    /// <inheritdoc />
    public TypeReference TypeReference => TypeReference.Actual.Date;
}
