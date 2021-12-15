namespace Reductech.EDR.Core;

/// <summary>
/// An array backed by an async collection
/// </summary>
public sealed record LazyArray<T>
    (IAsyncEnumerable<T> AsyncEnumerable) : Array<T>, IEquatable<Array<T>> where T : ISCLObject
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
    public override Array<T> Take(int count) => new LazyArray<T>(AsyncEnumerable.Take(count));

    /// <inheritdoc />
    public override Array<T> Skip(int count) => new LazyArray<T>(AsyncEnumerable.Skip(count));

    /// <inheritdoc />
    public override Task<Result<IArray, IError>> EnsureEvaluated(CancellationToken cancellation)
    {
        var r = Evaluate(cancellation)
            .Map(x => x as IArray);

        return r;
    }

    /// <inheritdoc />
    public override async Task<Result<EagerArray<T>, IError>> Evaluate(
        CancellationToken cancellation)
    {
        try
        {
            var list = await AsyncEnumerable.ToListAsync(cancellation);
            return new EagerArray<T>(list);
        }
        catch (ErrorException e)
        {
            return Result.Failure<EagerArray<T>, IError>(e.Error);
        }
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
    public override string Serialize(SerializeOptions options)
    {
        if (!options.EvaluateStreams)
            return "Stream";

        var list = GetElementsAsync(CancellationToken.None)
            .Result;

        if (list.IsSuccess)
        {
            if (list.Value.Count > options.MaxArrayLength)
                return $"{list.Value.Count} Elements";

            return SerializationMethods.SerializeList(
                list.Value
                    .Select(x => x.Serialize(options))
            );
        }

        return list.Error.AsString;
    }

    /// <inheritdoc />
    public override int GetHashCode() => GetHashCodeValue(this);

    bool IEquatable<Array<T>>.Equals(Array<T>? other) => Equals(this, other);

    /// <inheritdoc />
    protected override Type EqualityContract => typeof(Array<T>);

    /// <inheritdoc />
    public bool Equals(LazyArray<T>? other)
    {
        return Equals(this, other);
    }

    /// <inheritdoc />
    public override Result<Array<TElement>, IErrorBuilder> TryConvertElements<TElement>()
    {
        var stuff =
            GetAsyncEnumerable()
                .Select(x => x.TryConvert<TElement>())
                .Select(
                    x => x.IsFailure
                        ? throw new ErrorException(
                            x.Error.WithLocation(ErrorLocation.EmptyLocation)
                        ) //Todo correct error location
                        : x.Value
                )
                .ToSCLArray();

        return stuff;
    }

    /// <inheritdoc />
    public override Maybe<T1> MaybeAs<T1>()
    {
        if (this is T1 value)
            return value;

        return Maybe<T1>.None;
    }

    /// <inheritdoc />
    public override string ToString() => Serialize(SerializeOptions.Name);

    /// <inheritdoc />
    public override bool IsEvaluated => false;
}
