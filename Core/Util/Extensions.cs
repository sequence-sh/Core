namespace Reductech.EDR.Core.Util;

/// <summary>
/// SerializationMethods methods.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Gets a full message from an exception.
    /// </summary>
    public static string GetFullMessage(this Exception exception)
    {
        if (exception.InnerException == null)
            return exception.Message;

        var innerMessage = exception.InnerException.GetFullMessage();

        var message = $"{exception.Message}\r\n{innerMessage}";

        return message;
    }

    /// <summary>
    /// Gets the name of an enum value from the display attribute if it is present.
    /// </summary>
    public static string GetDisplayName(this Enum enumValue)
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First(x => x.MemberType == MemberTypes.Field)
            .GetCustomAttribute<DisplayAttribute>()
            ?
            .GetName() ?? enumValue.ToString();
    }

    /// <summary>
    /// Tries to get the element. Returns a failure if it is not present.
    /// </summary>
    public static Result<TValue> TryFindOrFail<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dictionary,
        TKey key,
        string? error)
    {
        var r = dictionary.TryFind(key);

        return r.ToResult(error ?? $"The element '{key}' was not present.");
    }

    /// <summary>
    /// Tries to get the element. Returns a failure if it is not present.
    /// </summary>
    public static Result<TValue, TError> TryFindOrFail<TKey, TValue, TError>(
        this IReadOnlyDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TError> error)
    {
        var r = dictionary.TryFind(key);

        if (r.HasValue)
            return r.GetValueOrThrow()!;

        return error()!;
    }

    /// <summary>
    /// Returns the elements of the sequence which are not null.
    /// </summary>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class =>
        (from val in source where val != null select val)!;

    /// <summary>
    /// Tries to get the first two elements of a sequence
    /// </summary>
    public static Maybe<(T first, T second)> GetFirstTwo<T>(this IEnumerable<T> enumerable)
    {
        var first  = Maybe<T>.None;
        var second = Maybe<T>.None;

        foreach (var t in enumerable.Take(2))
        {
            if (first.HasNoValue)
                first = t;
            else
                second = t;
        }

        if (first.HasValue && second.HasValue)
            return (first.GetValueOrThrow(), second.GetValueOrThrow());

        return Maybe<(T first, T second)>.None;
    }
}
