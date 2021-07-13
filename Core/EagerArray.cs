using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{

/// <summary>
/// An array backed by a list
/// </summary>
public sealed record EagerArray<T>(IReadOnlyList<T> List) : Array<T>, IEquatable<Array<T>>
{
    /// <summary>
    /// Empty constructor for reflection
    /// </summary>
    public EagerArray() : this(ImmutableList<T>.Empty) { }

    /// <inheritdoc />
    public override IAsyncEnumerable<T> GetAsyncEnumerable() => List.ToAsyncEnumerable();

    /// <inheritdoc />
    public override async Task<Result<bool, IError>> AnyAsync(CancellationToken cancellation)
    {
        await Task.CompletedTask;
        return List.Any();
    }

    /// <inheritdoc />
    public override async Task<Result<int, IError>> CountAsync(CancellationToken cancellation)
    {
        await Task.CompletedTask;
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
    public override async Task<Result<EagerArray<T>, IError>> Evaluate(
        CancellationToken cancellation)
    {
        await Task.CompletedTask;
        return this;
    }

    /// <inheritdoc />
    public override async Task<Result<Unit, IError>> ForEach(
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
    public override async Task<Result<int, IError>> IndexOfAsync(
        T element,
        CancellationToken cancellation)
    {
        await Task.CompletedTask;
        var index = List.ToList().IndexOf(element);

        return index;
    }

    /// <inheritdoc />
    public override async Task<Result<T, IError>> ElementAtAsync(
        int index,
        ErrorLocation location,
        CancellationToken cancellation)
    {
        await Task.CompletedTask;

        if (index < 0 || index >= List.Count)
            return new SingleError(location, ErrorCode.IndexOutOfBounds);

        return List[index];
    }

    /// <inheritdoc />
    public override async Task<Result<IReadOnlyList<T>, IError>> GetElementsAsync(
        CancellationToken cancellation)
    {
        await Task.CompletedTask;
        return Result.Success<IReadOnlyList<T>, IError>(List);
    }

    /// <inheritdoc />
    public override string NameInLogs => List.Count + " Elements";

    /// <inheritdoc />
    public override string Serialize =>
        SerializationMethods.SerializeList(
            List.Select(x => SerializationMethods.SerializeObject(x))
        );

    /// <inheritdoc />
    public override int GetHashCode() => GetHashCodeValue(this);

    bool IEquatable<Array<T>>.Equals(Array<T>? other) => Equals(this, other);

    /// <inheritdoc />
    public override string ToString()
    {
        switch (List.Count)
        {
            case 0:     return "Empty Eager Array";
            case <= 10: return "[" + string.Join(", ", List.Select(x => x!.ToString())) + "]";
            default:    return $"Eager Array with {List.Count} Elements";
        }
    }

    /// <inheritdoc />
    protected override Type EqualityContract => typeof(Array<T>);

    /// <inheritdoc />
    public bool Equals(EagerArray<T>? other)
    {
        return Equals(this, other);
    }
}

}
