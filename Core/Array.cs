namespace Reductech.Sequence.Core;

/// <summary>
/// Either a list or an asynchronous list
/// </summary>
public abstract record Array<T> : IArray where T : ISCLObject
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
    /// Take only the first n elements of the array
    /// </summary>
    public abstract Array<T> Take(int count);

    /// <summary>
    /// Skip the first n elements of the array
    /// </summary>
    public abstract Array<T> Skip(int count);

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

    /// <inheritdoc />
    public abstract bool IsEmpty();

    /// <summary>
    /// Returns some number of elements
    /// </summary>
    public Array<TResult> SelectMany<TResult>(Func<T, IAsyncEnumerable<TResult>> selector)
        where TResult : ISCLObject
    {
        var r = GetAsyncEnumerable().SelectMany(selector).ToSCLArray();
        return r;
    }

    /// <summary>
    /// Perform an action on every member of the sequence
    /// </summary>
    public Array<TResult> SelectAwait<TResult>(Func<T, ValueTask<TResult>> selector)
        where TResult : ISCLObject
    {
        var r = GetAsyncEnumerable().SelectAwait(selector).ToSCLArray();

        return r;
    }

    /// <summary>
    /// Group values by a selector
    /// </summary>
    public Array<Entity> GroupByAwait(Func<T, ValueTask<StringStream>> selector)
    {
        var r =
            GetAsyncEnumerable()
                .GroupByAwait(selector)
                .SelectAwait(
                    async
                        x =>
                    {
                        var values = await x.ToListAsync();

                        var e = Entity.Create(("Key", x.Key), ("Values", values.ToSCLArray()));

                        return e;
                    }
                )
                .ToSCLArray();

        return r;
    }

    /// <summary>
    /// Perform an action on every member of the sequence
    /// </summary>
    public Array<TResult> Select<TResult>(Func<T, TResult> selector) where TResult : ISCLObject
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

    /// <inheritdoc />
    public Task<Result<List<ISCLObject>, IError>> GetObjectsAsync(CancellationToken cancellation)
    {
        return GetElementsAsync(cancellation).Map(x => x.Cast<ISCLObject>().ToList());
    }

    /// <inheritdoc />
    public TypeReference GetTypeReference() =>
        new TypeReference.Array(TypeReference.Create(typeof(T)));

    /// <inheritdoc />
    public abstract string Serialize(SerializeOptions options);

    /// <inheritdoc />
    public abstract Result<Array<TElement>, IErrorBuilder> TryConvertElements<TElement>()
        where TElement : ISCLObject;

    /// <inheritdoc />
    public abstract Task<Result<IArray, IError>> EnsureEvaluated(CancellationToken cancellation);

    /// <inheritdoc />
    public abstract bool IsEvaluated { get; }

    /// <summary>
    /// Create an array by converting elements
    /// </summary>
    public static Result<Array<T>, IErrorBuilder> CreateByConverting(IArray array)
    {
        return array.TryConvertElements<T>();
    }

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
    public override string ToString() => Serialize(SerializeOptions.Name);

    /// <inheritdoc />
    public object ToCSharpObject()
    {
        return Evaluate(CancellationToken.None)
            .Result.Value.List.Select(x => x.ToCSharpObject())
            .ToList();
    }

    void ISerializable.Format(
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        var list = Evaluate(CancellationToken.None)
            .Result.Value
            .List; //This can throw an exception in theory but it is unlikely in reality

        if (list.Any(x => x is Entity or IArray))
        {
            indentationStringBuilder.AppendLine("[");
            indentationStringBuilder.Indent();

            indentationStringBuilder.AppendJoin(
                ",",
                true,
                list,
                sclObject => sclObject.Format(
                    indentationStringBuilder,
                    options,
                    remainingComments
                )
            );

            indentationStringBuilder.AppendLineMaybe();
            indentationStringBuilder.UnIndent();
            indentationStringBuilder.Append("]");
        }
        else
        {
            var line = "[" + string.Join(
                                 ", ",
                                 list.Select(x => x.Serialize(SerializeOptions.Serialize))
                             )
                           + "]";

            indentationStringBuilder.Append(line);
        }
    }

    /// <inheritdoc />
    public abstract Maybe<T1> MaybeAs<T1>() where T1 : ISCLObject;

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(
        string path,
        SchemaConversionOptions? schemaConversionOptions)
    {
        var list = (this as IArray).ListIfEvaluated();

        if (list.HasValue)
        {
            SchemaNode additionalItems = new TrueNode();

            for (var index = 0; index < list.Value.Count; index++)
            {
                var entityValue = list.Value[index];
                var n = entityValue.ToSchemaNode(path + $"[{index}]", schemaConversionOptions);
                additionalItems = additionalItems.Combine(n);
            }

            return new ArrayNode(
                EnumeratedValuesNodeData.Empty,
                new ItemsData(ImmutableList<SchemaNode>.Empty, additionalItems)
            );
        }

        return new ArrayNode(
            EnumeratedValuesNodeData.Empty,
            new ItemsData(ImmutableList<SchemaNode>.Empty, TrueNode.Instance)
        );
    }

    /// <inheritdoc />
    public IConstantFreezableStep ToConstantFreezableStep(TextLocation location) =>
        new SCLConstantFreezable<Array<T>>(this, location);
}

/// <summary>
/// Either a list of an asynchronous list
/// </summary>
public interface IArray : ISCLObject
{
    /// <summary>
    /// Try to get the elements of this list, as objects asynchronously.
    /// </summary>
    Task<Result<List<ISCLObject>, IError>> GetObjectsAsync(CancellationToken cancellation);

    /// <summary>
    /// Attempts to convert the elements of the array to the chosen type
    /// </summary>
    Result<Array<TElement>, IErrorBuilder> TryConvertElements<TElement>()
        where TElement : ISCLObject;

    /// <summary>
    /// Ensure that this array is evaluated
    /// </summary>
    [Pure]
    Task<Result<IArray, IError>> EnsureEvaluated(CancellationToken cancellation);

    /// <summary>
    /// Whether this array is evaluated
    /// </summary>
    bool IsEvaluated { get; }

    /// <summary>
    /// This as a list, if it is evaluated
    /// </summary>
    /// <returns></returns>
    public Maybe<List<ISCLObject>> ListIfEvaluated()
    {
        if (!IsEvaluated)
            return Maybe<List<ISCLObject>>.None;

        var l = GetObjectsAsync(CancellationToken.None).Result.Value;
        return l;
    }
}

/// <summary>
/// Provides extension methods for converting Enumerables to Sequences
/// </summary>
public static class ArrayHelper
{
    /// <summary>
    /// Converts an enumerable to a Sequence
    /// </summary>
    public static Array<T> ToSCLArray<T>(this IAsyncEnumerable<T> enumerable)
        where T : ISCLObject => new LazyArray<T>(enumerable);

    /// <summary>
    /// Converts an asyncEnumerable to a Sequence
    /// </summary>
    public static Array<T> ToSCLArray<T>(this IEnumerable<T> enumerable) where T : ISCLObject =>
        new EagerArray<T>(enumerable.ToList());

    /// <summary>
    /// Creates an array from an enumerable of objects
    /// </summary>
    public static Array<T> CreateArray<T>(IEnumerable<object> elementsEnum) where T : ISCLObject
    {
        var r = new EagerArray<T>(elementsEnum.Cast<T>().ToList());
        return r;
    }
}
