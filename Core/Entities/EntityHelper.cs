namespace Sequence.Core.Entities;

/// <summary>
/// Contains methods for helping with entities.
/// </summary>
public static class EntityHelper
{
    /// <summary>
    /// Tries to convert an object into one suitable as an entity property.
    /// </summary>
    [Pure]
    public static async ValueTask<Result<ISCLObject, IError>> TryUnpackObjectAsync(
        ISCLObject o,
        CancellationToken cancellation)
    {
        if (o is IArray list)
        {
            var r = await list.GetObjectsAsync(cancellation);

            if (r.IsFailure)
                return r.ConvertFailure<ISCLObject>();

            var q = await r.Value.Select(x => TryUnpackObjectAsync(x, cancellation))
                .Combine(ErrorList.Combine)
                .Map(x => x.ToSCLArray() as ISCLObject);

            return q;
        }

        return Result.Success<ISCLObject, IError>(o);
    }
}
