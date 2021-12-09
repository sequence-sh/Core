using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core;

/// <summary>
/// Contains methods for helping with entities.
/// </summary>
public static class EntityHelper
{
    /// <summary>
    /// Tries to convert an object into one suitable as an entity property.
    /// </summary>
    [Pure]
    public static async Task<Result<object?, IError>> TryUnpackObjectAsync(
        object? o,
        CancellationToken cancellation)
    {
        if (o is IArray list)
        {
            var r = await list.GetObjectsAsync(cancellation);

            if (r.IsFailure)
                return r.ConvertFailure<object?>();

            var q = await r.Value.Select(x => TryUnpackObjectAsync(x, cancellation))
                .Combine(ErrorList.Combine)
                .Map(x => x.ToList());

            return q;
        }

        return Result.Success<object?, IError>(o);
    }
}
