using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{

/// <summary>
/// Either a list of an asynchronous list
/// </summary>
public abstract record Array<T> : IArray
{
    /// <summary>
    /// The empty array
    /// </summary>
    public static Array<T> Empty { get; } = new EagerArray<T>(ImmutableArray<T>.Empty);

    /// <summary>
    /// Gets this array as an async enumerable
    /// </summary>
    /// <returns></returns>
    public abstract IAsyncEnumerable<T> GetAsyncEnumerable();

    /// <summary>
    /// Does this list contain any elements
    /// </summary>
    public abstract Task<Result<bool, IError>> AnyAsync(CancellationToken cancellation);

    /// <summary>
    /// Returns the number of elements in the list
    /// </summary>
    public abstract Task<Result<int, IError>> CountAsync(CancellationToken cancellation);

    /// <summary>
    /// Change the order of elements of the array
    /// </summary>
    public abstract Array<T> Sort(bool descending);

    /// <summary>
    /// Evaluate the array - reading the results to memory
    /// </summary>
    public abstract Task<Result<EagerArray<T>, IError>> Evaluate(CancellationToken cancellation);

    /// <summary>
    /// Change the ordering of the Array
    /// </summary>
    public Array<T> Sort<TKey>(
        bool descending,
        Func<T, CancellationToken, ValueTask<TKey>> func)
    {
        var asyncEnumerable1 = GetAsyncEnumerable();

        IAsyncEnumerable<T> asyncEnumerable2 = descending
            ? asyncEnumerable1.OrderByDescendingAwaitWithCancellation(func)
            : asyncEnumerable1.OrderByAwaitWithCancellation(func);

        return new LazyArray<T>(asyncEnumerable2);
    }

    /// <summary>
    /// Apply a function for each element of the sequence
    /// </summary>
    public abstract Task<Result<Unit, IError>> ForEach(
        Func<T, CancellationToken, ValueTask<Result<Unit, IError>>> func,
        CancellationToken cancellation);

    /// <summary>
    /// Returns some number of elements
    /// </summary>
    public Array<TResult> SelectMany<TResult>(Func<T, IAsyncEnumerable<TResult>> selector)
    {
        var r = GetAsyncEnumerable().SelectMany(selector).ToSCLArray();
        return r;
    }

    /// <summary>
    /// Perform an action on every member of the sequence
    /// </summary>
    public Array<TResult> SelectAwait<TResult>(Func<T, ValueTask<TResult>> selector)
    {
        var r = GetAsyncEnumerable().SelectAwait(selector).ToSCLArray();

        return r;
    }

    /// <summary>
    /// Perform an action on every member of the sequence
    /// </summary>
    public Array<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        var r = GetAsyncEnumerable().Select(selector).ToSCLArray();
        return r;
    }

    /// <summary>
    /// Get the first index of an element
    /// </summary>
    public abstract Task<Result<int, IError>> IndexOfAsync(
        T element,
        CancellationToken cancellation);

    /// <summary>
    /// Try to get the element at a particular index
    /// </summary>
    public abstract Task<Result<T, IError>> ElementAtAsync(
        int index,
        ErrorLocation location,
        CancellationToken cancellation);

    /// <summary>
    /// Gets the elements from the list asynchronously
    /// </summary>
    public abstract Task<Result<IReadOnlyList<T>, IError>> GetElementsAsync(
        CancellationToken cancellation);

    ///// <inheritdoc />
    //public Result<List<object>, IError> GetObjects() =>
    //    GetElements().Map(x => x.Cast<object>().ToList());

    /// <inheritdoc />
    public Task<Result<List<object>, IError>> GetObjectsAsync(CancellationToken cancellation)
    {
        return GetElementsAsync(cancellation).Map(x => x.Cast<object>().ToList());
    }

    /// <inheritdoc />
    public abstract string NameInLogs { get; }

    /// <summary>
    /// Equality comparison
    /// </summary>
    protected static bool Equals(Array<T> a1, Array<T>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(a1, other))
            return true;

        var l1 = a1.GetElementsAsync(CancellationToken.None).Result;
        var l2 = other.GetElementsAsync(CancellationToken.None).Result;

        if (l1.IsFailure && l2.IsFailure)
            return l1.Error.Equals(l2.Error);

        if (l1.IsFailure || l2.IsFailure)
            return false;

        return l1.Value.SequenceEqual(l2.Value);
    }

    /// <summary>
    /// Gets the hash code value
    /// </summary>
    protected static int GetHashCodeValue(Array<T> a1)
    {
        var e = a1.GetElementsAsync(CancellationToken.None).Result;

        if (e.IsFailure)
            return e.Error.GetHashCode();

        if (e.Value.Count == 0)
            return 0;

        return HashCode.Combine(e.Value[0], e.Value.Count);
    }

    /// <inheritdoc />
    public override string ToString() => NameInLogs;
}

/// <summary>
/// Either a list of an asynchronous list
/// </summary>
public interface IArray
{
    /// <summary>
    /// Try to get the elements of this list, as objects asynchronously.
    /// </summary>
    Task<Result<List<object>, IError>> GetObjectsAsync(CancellationToken cancellation);

    /// <summary>
    /// How this Array will appear in the logs.
    /// </summary>
    public string NameInLogs { get; }
}

/// <summary>
/// Provides extension methods for converting Enumerables to Sequences
/// </summary>
public static class ArrayHelper
{
    /// <summary>
    /// Converts an enumerable to a Sequence
    /// </summary>
    public static Array<T> ToSCLArray<T>(this IAsyncEnumerable<T> enumerable) =>
        new LazyArray<T>(enumerable);

    /// <summary>
    /// Converts an asyncEnumerable to a Sequence
    /// </summary>
    public static Array<T> ToSCLArray<T>(this IEnumerable<T> enumerable) =>
        new EagerArray<T>(enumerable.ToList());

    /// <summary>
    /// Creates an array from an enumerable of objects
    /// </summary>
    public static Array<T> CreateArray<T>(IEnumerable<object> elementsEnum)
    {
        var r = new EagerArray<T>(elementsEnum.Cast<T>().ToList());
        return r;
    }
}

}
