using Namotion.Reflection;

namespace Reductech.EDR.Core.Util;

/// <summary>
/// Functional methods
/// </summary>
public static class FunctionalExtensions
{
    /// <summary>
    /// Converts this object to type T. Returns failure if the cast fails.
    /// </summary>
    public static Result<T, IErrorBuilder> TryConvert<T>(this object? obj)
    {
        if (obj is T objAsT)
            return objAsT;

        if (typeof(T).IsGenericType
         && typeof(T).GetGenericTypeDefinition().Name.StartsWith("OneOf"))
        {
            for (var index = 0; index < typeof(T).GenericTypeArguments.Length; index++)
            {
                var genericTypeArgument = typeof(T).GenericTypeArguments[index];

                if (genericTypeArgument.IsInstanceOfType(obj))
                {
                    var fromMethod = typeof(T).GetMethod(
                        $"FromT{index}",
                        BindingFlags.Static | BindingFlags.Public
                    );

                    var oneOf = fromMethod?.Invoke(null, new[] { obj });

                    if (oneOf is T oneOfAsT)
                        return oneOfAsT;
                }
            }
        }

        try
        {
            var converted = Convert.ChangeType(obj, typeof(T));

            if (converted is T objConverted)
                return objConverted;
        }
        catch (InvalidCastException) { }

        return ErrorCode.InvalidCast.ToErrorBuilder(typeof(T).GetDisplayName(), obj ?? "null");
    }

    /// <summary>
    /// Get the value or throw an exception
    /// </summary>
    public static T GetOrThrow<T>(this Result<T, IErrorBuilder> result)
    {
        if (result.IsSuccess)
            return result.Value;

        throw new Exception(result.Error.AsString);
    }

    /// <summary>
    /// Get the value or throw an exception
    /// </summary>
    public static T GetOrThrow<T>(this Result<T, IError> result)
    {
        if (result.IsSuccess)
            return result.Value;

        throw new Exception(result.Error.AsString);
    }

    /// <summary>
    /// Casts the result to type T2.
    /// Returns failure if this cast is not possible.
    /// </summary>
    public static Result<T2, TE>
        BindCast<T1, T2, TE>(this Result<T1, TE> result, TE error)
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
    /// Create a tuple with 5 results.
    /// Func2 will not be evaluated unless result1 is success and so on.
    /// </summary>
    public static Result<(T1, T2, T3, T4, T5), TE> Compose<T1, T2, T3, T4, T5, TE>(
        this Result<T1, TE> result1,
        Func<Result<T2, TE>> func2,
        Func<Result<T3, TE>> func3,
        Func<Result<T4, TE>> func4,
        Func<Result<T5, TE>> func5)
    {
        if (result1.IsFailure)
            return result1.ConvertFailure<(T1, T2, T3, T4, T5)>();

        var result2 = func2();

        if (result2.IsFailure)
            return result2.ConvertFailure<(T1, T2, T3, T4, T5)>();

        var result3 = func3();

        if (result3.IsFailure)
            return result3.ConvertFailure<(T1, T2, T3, T4, T5)>();

        var result4 = func4();

        if (result4.IsFailure)
            return result4.ConvertFailure<(T1, T2, T3, T4, T5)>();

        var result5 = func5();

        if (result5.IsFailure)
            return result5.ConvertFailure<(T1, T2, T3, T4, T5)>();

        return (result1.Value, result2.Value, result3.Value, result4.Value, result5.Value);
    }

    /// <summary>
    /// Create a tuple with 5 results.
    /// Func2 will not be evaluated unless result1 is success and so on.
    /// </summary>
    public static Result<(T1, T2, T3), TE> Compose<T1, T2, T3, TE>(
        this Result<T1, TE> result1,
        Func<Result<T2, TE>> func2,
        Func<Result<T3, TE>> func3)
    {
        if (result1.IsFailure)
            return result1.ConvertFailure<(T1, T2, T3)>();

        var result2 = func2();

        if (result2.IsFailure)
            return result2.ConvertFailure<(T1, T2, T3)>();

        var result3 = func3();

        if (result3.IsFailure)
            return result3.ConvertFailure<(T1, T2, T3)>();

        return (result1.Value, result2.Value, result3.Value);
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
            yield return maybe.GetValueOrThrow();
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
