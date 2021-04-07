using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{

/// <summary>
/// Either a list of an asynchronous list
/// </summary>
public sealed class Array<T> : IArray, IEquatable<Array<T>>
{
    /// <summary>
    /// Create an empty array
    /// </summary>
    public Array()
    {
        Option = OneOf<IReadOnlyList<T>, IAsyncEnumerable<T>>.FromT0(ImmutableList<T>.Empty);
    }

    /// <summary>
    /// Create a new Array
    /// </summary>
    public Array(IReadOnlyList<T> list) =>
        Option = OneOf<IReadOnlyList<T>, IAsyncEnumerable<T>>.FromT0(list);

    /// <summary>
    /// Create a new Array
    /// </summary>
    public Array(IAsyncEnumerable<T> asyncEnumerable) =>
        Option = OneOf<IReadOnlyList<T>, IAsyncEnumerable<T>>.FromT1(asyncEnumerable);

    /// <summary>
    /// The option.
    /// </summary>
    public OneOf<IReadOnlyList<T>, IAsyncEnumerable<T>> Option { get; }

    /// <summary>
    /// Does this list contain any elements
    /// </summary>
    public async Task<Result<bool, IError>> AnyAsync(CancellationToken cancellation)
    {
        if (Option.IsT0)
            return Option.AsT0.Any();

        var r = await TryRun(Option.AsT1, (x, c) => x.AnyAsync(c), cancellation);

        return r;
    }

    /// <summary>
    /// Returns the number of elements in the list
    /// </summary>
    public async Task<Result<int, IError>> CountAsync(CancellationToken cancellation)
    {
        if (Option.IsT0)
            return Option.AsT0.Count;

        var r = await TryRun(Option.AsT1, (x, c) => x.CountAsync(c), cancellation);

        return r;
    }

    /// <summary>
    /// Change the order of elements of the array
    /// </summary>
    public Array<T> Sort(bool descending)
    {
        if (Option.IsT0)
        {
            IEnumerable<T> enumerable;

            if (descending)
                enumerable = Option.AsT0.OrderByDescending(x => x);
            else
                enumerable = Option.AsT0.OrderBy(x => x);

            return new Array<T>(enumerable.ToList());
        }

        IAsyncEnumerable<T> asyncEnumerable;

        if (descending)
            asyncEnumerable = Option.AsT1.OrderByDescending(x => x);
        else
            asyncEnumerable = Option.AsT1.OrderBy(x => x);

        return new Array<T>(asyncEnumerable);
    }

    /// <summary>
    /// Change the ordering of the Array
    /// </summary>
    public Array<T> Sort<TKey>(bool descending, Func<T, CancellationToken, ValueTask<TKey>> func)
    {
        IAsyncEnumerable<T> asyncEnumerable1 =
            Option.IsT0 ? Option.AsT0.ToAsyncEnumerable() : Option.AsT1;

        IAsyncEnumerable<T> asyncEnumerable2;

        if (descending)
            asyncEnumerable2 = asyncEnumerable1.OrderByDescendingAwaitWithCancellation(func);
        else
            asyncEnumerable2 = asyncEnumerable1.OrderByAwaitWithCancellation(func);

        return new Array<T>(asyncEnumerable2);
    }

    /// <summary>
    /// Apply a function for each element of the sequence
    /// </summary>
    public async Task<Result<Unit, IError>> ForEach(
        Func<T, CancellationToken, ValueTask<Result<Unit, IError>>> func,
        CancellationToken cancellation)
    {
        var errors = new List<IError>();

        if (Option.IsT0)
        {
            foreach (var t in Option.AsT0)
            {
                var r = await func(t, cancellation);

                if (r.IsFailure)
                    errors.Add(r.Error);
            }
        }
        else
        {
            try
            {
                await foreach (var t in Option.AsT1.WithCancellation(cancellation))
                {
                    var r = await func(t, cancellation);

                    if (r.IsFailure)
                        errors.Add(r.Error);
                }
            }
            catch (ErrorException e)
            {
                errors.Add(e.Error);
            }
        }

        if (errors.Any())
            return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

        return Unit.Default;
    }

    /// <summary>
    /// Returns some number of elements
    /// </summary>
    public Array<TResult> SelectMany<TResult>(Func<T, IAsyncEnumerable<TResult>> selector)
    {
        IAsyncEnumerable<T> asyncEnumerable =
            Option.IsT0 ? Option.AsT0.ToAsyncEnumerable() : Option.AsT1;

        var r = asyncEnumerable.SelectMany(selector).ToSCLArray();

        return r;
    }

    /// <summary>
    /// Perform an action on every member of the sequence
    /// </summary>
    public Array<TResult> SelectAwait<TResult>(Func<T, ValueTask<TResult>> selector)
    {
        IAsyncEnumerable<T> asyncEnumerable =
            Option.IsT0 ? Option.AsT0.ToAsyncEnumerable() : Option.AsT1;

        var r = asyncEnumerable.SelectAwait(selector).ToSCLArray();

        return r;
    }

    /// <summary>
    /// Perform an action on every member of the sequence
    /// </summary>
    public Array<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        IAsyncEnumerable<T> asyncEnumerable =
            Option.IsT0 ? Option.AsT0.ToAsyncEnumerable() : Option.AsT1;

        var r = asyncEnumerable.Select(selector).ToSCLArray();

        return r;
    }

    /// <summary>
    /// Get the first index of an element
    /// </summary>
    public async Task<Result<int, IError>> IndexOfAsync(T element, CancellationToken cancellation)
    {
        if (Option.IsT0)
        {
            var index = Option.AsT0.ToList().IndexOf(element);

            return index;
        }

        var list = await TryRun(Option.AsT1, (x, c) => x.ToListAsync(c), cancellation)
            .Map(x => x.IndexOf(element));

        return list;
    }

    /// <summary>
    /// Try to get the element at a particular index
    /// </summary>
    public async Task<Result<T, IError>> ElementAtAsync(
        int index,
        ErrorLocation location,
        CancellationToken cancellation)
    {
        if (index < 0)
            return new SingleError(location, ErrorCode.IndexOutOfBounds);

        if (Option.TryPickT0(out var list, out var asyncList))
        {
            if (index >= list.Count)
                return new SingleError(location, ErrorCode.IndexOutOfBounds);

            return Option.AsT0[index];
        }

        try
        {
            var r = await TryRun(
                asyncList,
                (x, c) => x.ElementAtAsync(index, c),
                cancellation
            );

            return r;
        }
        catch (IndexOutOfRangeException)
        {
            return new SingleError(location, ErrorCode.IndexOutOfBounds);
        }
    }

    /// <summary>
    /// Gets the elements from the list asynchronously
    /// </summary>
    public async Task<Result<IReadOnlyList<T>, IError>> GetElementsAsync(
        CancellationToken cancellation)
    {
        if (Option.IsT0)
            return Result.Success<IReadOnlyList<T>, IError>(Option.AsT0);

        var r = await TryRun<List<T>>(Option.AsT1, (x, c) => x.ToListAsync(c), cancellation);

        if (r.IsFailure)
            return r.ConvertFailure<IReadOnlyList<T>>();

        return r.Value;
    }

    /// <summary>
    /// Gets the elements from the list
    /// </summary>
    public Result<IReadOnlyList<T>, IError> GetElements()
    {
        if (Option.IsT0)
            return Result.Success<IReadOnlyList<T>, IError>(Option.AsT0);

        var r = TryRun<List<T>>(Option.AsT1, (x, c) => x.ToListAsync(c), CancellationToken.None)
            .Result;

        if (r.IsFailure)
            return r.ConvertFailure<IReadOnlyList<T>>();

        return r.Value;
    }

    private static async Task<Result<TResult, IError>> TryRun<TResult>(
        IAsyncEnumerable<T> asyncEnumerable,
        Func<IAsyncEnumerable<T>, CancellationToken, ValueTask<TResult>> func,
        CancellationToken cancellation)
    {
        try
        {
            var r = await func(asyncEnumerable, cancellation);

            return r;
        }
        catch (ErrorException e)
        {
            return Result.Failure<TResult, IError>(e.Error);
        }
    }

    /// <inheritdoc />
    public Result<List<object>, IError> GetObjects() =>
        GetElements().Map(x => x.Cast<object>().ToList());

    /// <inheritdoc />
    public Task<Result<List<object>, IError>> GetObjectsAsync(CancellationToken cancellation)
    {
        return GetElementsAsync(cancellation).Map(x => x.Cast<object>().ToList());
    }

    /// <inheritdoc />
    public string NameInLogs => Option.Match(x => x.Count + " Elements", _ => "Stream");

    /// <inheritdoc />
    public bool Equals(Array<T>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        var l1 = GetElements();
        var l2 = other.GetElements();

        if (l1.IsFailure && l2.IsFailure)
            return l1.Error.Equals(l2.Error);

        if (l1.IsFailure || l2.IsFailure)
            return false;

        return l1.Value.SequenceEqual(l2.Value);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var e = GetElements();

        if (e.IsFailure)
            return e.Error.GetHashCode();

        if (e.Value.Count == 0)
            return 0;

        return HashCode.Combine(e.Value[0], e.Value.Count);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj is Array<T> al)
            return Equals(al);

        return false;
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
    /// Try to get the elements of this list, as objects.
    /// </summary>
    Result<List<object>, IError> GetObjects();

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
    public static Array<T> ToSCLArray<T>(this IAsyncEnumerable<T> enumerable) => new(enumerable);

    /// <summary>
    /// Converts an asyncEnumerable to a Sequence
    /// </summary>
    public static Array<T> ToSCLArray<T>(this IEnumerable<T> enumerable) =>
        new(enumerable.ToList());

    /// <summary>
    /// Creates an array from an enumerable of objects
    /// </summary>
    public static Array<T> CreateArray<T>(IEnumerable<object> elementsEnum)
    {
        var r = new Array<T>(elementsEnum.Cast<T>().ToList());
        return r;
    }
}

}
