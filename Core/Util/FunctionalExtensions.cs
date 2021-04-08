using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Util
{

/// <summary>
/// Functional methods
/// </summary>
public static class FunctionalExtensions
{
    /// <summary>
    /// Converts this object to type T. Returns failure if the cast fails.
    /// </summary>
    public static Result<T, IErrorBuilder> TryConvert<T>(this object obj)
    {
        if (obj is T objAsT)
            return objAsT;

        var converted = Convert.ChangeType(obj, typeof(T));

        if (converted is T objConverted)
            return objConverted;

        return ErrorCode.InvalidCast.ToErrorBuilder(obj, typeof(T).Name);
    }

    /// <summary>
    /// Casts the result to type T2.
    /// Returns failure if this cast is not possible.
    /// </summary>
    public static Result<T2, TE>
        BindCast<T1, T2, TE>(this Result<T1, TE> result, TE error) // where T2 : T1
    {
        if (result.IsFailure)
            return result.ConvertFailure<T2>();

        if (result.Value is T2 t2)
            return t2;

        return Result.Failure<T2, TE>(error);
    }

    /// <summary>
    /// Casts the result to type T2.
    /// Returns failure if this cast is not possible.
    /// </summary>
    public static async Task<Result<T2, TE>> BindCast<T1, T2, TE>(
        this Task<Result<T1, TE>> result,
        TE error) // where T2 : T1
    {
        var result1 = await result;
        return result1.BindCast<T1, T2, TE>(error);
    }

    /// <summary>
    /// Create a tuple with 2 results.
    /// Func2 will not be evaluated unless result1 is success.
    /// </summary>
    public static Result<(T1, T2), TE> Compose<T1, T2, TE>(
        this Result<T1, TE> result1,
        Func<Result<T2, TE>> func2)
    {
        if (result1.IsFailure)
            return result1.ConvertFailure<(T1, T2)>();

        var result2 = func2();

        if (result2.IsFailure)
            return result2.ConvertFailure<(T1, T2)>();

        return (result1.Value, result2.Value);
    }

    /// <summary>
    /// Converts a maybe to an enumerable.
    /// </summary>
    public static IEnumerable<T> ToEnumerable<T>(this Maybe<T> maybe)
    {
        if (maybe.HasValue)
            yield return maybe.Value;
    }

    /// <summary>
    /// Gets the value if the result was successful
    /// </summary>
    public static Maybe<T> ToMaybe<T, TError>(this Result<T, TError> result)
    {
        if (result.IsSuccess)
            return result.Value;

        return Maybe<T>.None;
    }
}

}
