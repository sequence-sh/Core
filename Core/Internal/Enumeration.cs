using System;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A member of a set of predefined values
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed record Enumeration(string Type, string Value)
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <inheritdoc />
    public override string ToString() => Type + "." + Value;

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

}
