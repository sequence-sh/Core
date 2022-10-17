namespace Reductech.Sequence.Core;

/// <summary>
/// An array backed by a list
/// </summary>
public sealed record EagerArray<T>(IReadOnlyList<T> List) : Array<T>, IEquatable<Array<T>>
    where T : ISCLObject
{
    /// <summary>
    /// Empty constructor for reflection
    /// </summary>
    public EagerArray() : this(ImmutableList<T>.Empty) { }

    /// <inheritdoc />
    public override IAsyncEnumerable<T> GetAsyncEnumerable() => List.ToAsyncEnumerable();

    /// <inheritdoc />
    #pragma warning disable CS1998
    public override async ValueTask<Result<bool, IError>> AnyAsync(CancellationToken cancellation)
        #pragma warning restore CS1998
    {
        return List.Any();
    }

    /// <inheritdoc />
    #pragma warning disable CS1998
    public override async ValueTask<Result<int, IError>> CountAsync(CancellationToken cancellation)
        #pragma warning restore CS1998
    {
        return List.Count;
    }

    /// <inheritdoc />
    public override Array<T> Sort(bool descending)
    {
        IEnumerable<T> enumerable;

        if (descending)
            enumerable = List.OrderByDescending(x => x);
        else
            enumerable = List.OrderBy(x => x);

        return new EagerArray<T>(enumerable.ToList());
    }

    /// <inheritdoc />
    public override Array<T> Take(int count)
    {
        return new EagerArray<T>(List.Take(count).ToList());
    }

    /// <inheritdoc />
    public override Array<T> Skip(int count)
    {
        return new EagerArray<T>(List.Skip(count).ToList());
    }

    /// <inheritdoc />
    #pragma warning disable CS1998
    public override async ValueTask<Result<EagerArray<T>, IError>> Evaluate(
        #pragma warning restore CS1998
        CancellationToken cancellation)
    {
        return this;
    }

    /// <inheritdoc />
    public override async ValueTask<Result<Unit, IError>> ForEach(
        Func<T, CancellationToken, ValueTask<Result<Unit, IError>>> func,
        CancellationToken cancellation)
    {
        var errors = new List<IError>();

        foreach (var t in List)
        {
            var r = await func(t, cancellation);

            if (r.IsFailure)
                errors.Add(r.Error);
        }

        if (errors.Any())
            return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

        return Unit.Default;
    }

    /// <inheritdoc />
    #pragma warning disable CS1998
    public override async ValueTask<Result<int, IError>> IndexOfAsync(
        #pragma warning restore CS1998
        T element,
        CancellationToken cancellation)
    {
        var index = List.ToList().IndexOf(element);

        return index;
    }

    /// <inheritdoc />
    #pragma warning disable CS1998
    public override async ValueTask<Result<T, IError>> ElementAtAsync(
        #pragma warning restore CS1998
        int index,
        ErrorLocation location,
        CancellationToken cancellation)
    {
        if (index < 0 || index >= List.Count)
            return new SingleError(location, ErrorCode.IndexOutOfBounds);

        return List[index];
    }

    /// <inheritdoc />
    #pragma warning disable CS1998
    public override async ValueTask<Result<IReadOnlyList<T>, IError>> GetElementsAsync(
        #pragma warning restore CS1998
        CancellationToken cancellation)
    {
        return Result.Success<IReadOnlyList<T>, IError>(List);
    }

    /// <inheritdoc />
    public override int GetHashCode() => GetHashCodeValue(this);

    bool IEquatable<Array<T>>.Equals(Array<T>? other) => Equals(this, other);

    /// <inheritdoc />
    #pragma warning disable CS1998
    public override async ValueTask<Result<IArray, IError>> EnsureEvaluated(
        #pragma warning restore CS1998
        CancellationToken cancellation)
    {
        return this;
    }

    /// <inheritdoc />
    public override string ToString() => Serialize(SerializeOptions.Name);

    /// <inheritdoc />
    protected override Type EqualityContract => typeof(Array<T>);

    /// <inheritdoc />
    public bool Equals(EagerArray<T>? other)
    {
        return Equals(this, other);
    }

    /// <inheritdoc />
    public override string Serialize(SerializeOptions options)
    {
        if (List.Count > options.MaxArrayLength)
            return $"{List.Count} Elements";

        return SerializationMethods.SerializeList(List.Select(x => x.Serialize(options)));
    }

    /// <inheritdoc />
    public override Result<Array<TElement>, IErrorBuilder> TryConvertElements<TElement>()
    {
        return List
            .Select(x => x.TryConvertTyped<TElement>("Element"))
            .Combine(ErrorBuilderList.Combine)
            .Map(x => x.ToSCLArray());
    }

    /// <inheritdoc />
    public override bool IsEvaluated => true;

    /// <inheritdoc />
    public override Maybe<T1> MaybeAs<T1>()
    {
        if (this is T1 value)
            return value;

        if (typeof(T1).IsGenericType && typeof(T1).GetGenericTypeDefinition() == typeof(Array<>))
        {
            var memberType = typeof(T1).GenericTypeArguments[0];

            var method =
                typeof(EagerArray<T>).GetMethod(
                        "TryConvertMembers",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    )!
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(memberType);

            var result = (Maybe<IArray>)method.Invoke(this, null)!;

            if (result.HasValue && result.Value is T1 t1)
                return t1;
        }

        return Maybe<T1>.None;
    }

    /// <inheritdoc />
    public override bool IsEmpty() => !List.Any();

    private Maybe<IArray> TryConvertMembers<TMember>() where TMember : ISCLObject
    {
        //This METHOD is used with reflection
        var newList = new List<TMember>(this.List.Count);

        foreach (var sclObject in List)
        {
            var m = sclObject.MaybeAs<TMember>();

            if (m.HasValue)
                newList.Add(m.Value);
            else
                return Maybe<IArray>.None;
        }

        return newList.ToSCLArray();
    }
}
