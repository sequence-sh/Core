using System;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A member of a set of predefined values
/// </summary>
public sealed record Enumeration(string Type, string Value)
{
    /// <inheritdoc />
    public override string ToString() => Serialize;

    /// <summary>
    /// Serialize this enumeration
    /// </summary>
    public string Serialize => Type + "." + Value;

    /// <summary>
    /// Try to convert this to a C# enum
    /// </summary>
    public Maybe<T> TryConvert<T>() where T : struct, Enum
    {
        if (Enum.TryParse(Value, true, out T t))
            return t;

        return Maybe<T>.None;
    }
}
