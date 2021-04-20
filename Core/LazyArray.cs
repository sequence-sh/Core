using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{

/// <summary>
/// An array backed by an async collection
/// </summary>
public sealed record LazyArray<T>
    (IAsyncEnumerable<T> AsyncEnumerable) : Array<T>, IEquatable<Array<T>>
{
    /// <inheritdoc />
    public override IAsyncEnumerable<T> GetAsyncEnumerable() => AsyncEnumerable;

    /// <inheritdoc />
    public override Task<Result<bool, IError>> AnyAsync(CancellationToken cancellation)
    {
        return TryRun(AsyncEnumerable, (x, c) => x.AnyAsync(c), cancellation);
    }

    /// <inheritdoc />
    public override Task<Result<int, IError>> CountAsync(CancellationToken cancellation)
    {
        return TryRun(AsyncEnumerable, (x, c) => x.CountAsync(c), cancellation);
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
    public override Array<T> Sort(bool descending)
    {
        IAsyncEnumerable<T> asyncEnumerable = descending
            ? AsyncEnumerable.OrderByDescending(x => x)
            : AsyncEnumerable.OrderBy(x => x);

        return new LazyArray<T>(asyncEnumerable);
    }

    /// <inheritdoc />
    public override async Task<Result<Unit, IError>> ForEach(
        Func<T, CancellationToken, ValueTask<Result<Unit, IError>>> func,
        CancellationToken cancellation)
    {
        var errors = new List<IError>();

        try
        {
            await foreach (var t in AsyncEnumerable.WithCancellation(cancellation))
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

        if (errors.Any())
            return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

        return Unit.Default;
    }

    /// <inheritdoc />
    public override async Task<Result<int, IError>> IndexOfAsync(
        T element,
        CancellationToken cancellation)
    {
        var list = await TryRun(AsyncEnumerable, (x, c) => x.ToListAsync(c), cancellation)
            .Map(x => x.IndexOf(element));

        return list;
    }

    /// <inheritdoc />
    public override async Task<Result<T, IError>> ElementAtAsync(
        int index,
        ErrorLocation location,
        CancellationToken cancellation)
    {
        try
        {
            var r = await TryRun(
                AsyncEnumerable,
                (x, c) => x.ElementAtAsync(index, c),
                cancellation
            );

            return r;
        }
        catch (ArgumentOutOfRangeException)
        {
            return new SingleError(location, ErrorCode.IndexOutOfBounds);
        }
    }

    /// <inheritdoc />
    public override async Task<Result<IReadOnlyList<T>, IError>> GetElementsAsync(
        CancellationToken cancellation)
    {
        var r = await TryRun(AsyncEnumerable, (x, c) => x.ToListAsync(c), cancellation);

        if (r.IsFailure)
            return r.ConvertFailure<IReadOnlyList<T>>();

        return r.Value;
    }

    /// <inheritdoc />
    public override string NameInLogs => "Stream";

    /// <inheritdoc />
    public override int GetHashCode() => GetHashCodeValue(this);

    bool IEquatable<Array<T>>.Equals(Array<T>? other) => Equals(this, other);

    /// <inheritdoc />
    public bool Equals(LazyArray<T>? other)
    {
        return Equals(this, other);
    }
}

}
