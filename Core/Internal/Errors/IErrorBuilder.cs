using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// An error without a location.
/// </summary>
public interface IErrorBuilder
{
    /// <summary>
    /// Converts this errorBuilder to an error
    /// </summary>
    [Pure]
    public IError WithLocation(IErrorLocation location);

    /// <summary>
    /// The error builders.
    /// </summary>
    public IEnumerable<ErrorBuilder> GetErrorBuilders();

    /// <summary>
    /// String representation of this ErrorBuilder.
    /// </summary>
    public string AsString { get; }
}

}
