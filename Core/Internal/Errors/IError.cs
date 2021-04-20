using System;
using System.Collections.Generic;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// One or more errors caused
/// </summary>
public interface IError : IEquatable<IError>
{
    /// <summary>
    /// The errors.
    /// </summary>
    IEnumerable<SingleError> GetAllErrors();

    /// <summary>
    /// The error as a string.
    /// Does not include the location
    /// </summary>
    string AsString { get; }

    /// <summary>
    /// The error as a string.
    /// Includes the location
    /// </summary>
    string AsStringWithLocation { get; }

    /// <summary>
    /// Converts this Error to an ErrorBuilder
    /// </summary>
    IErrorBuilder ToErrorBuilder { get; }
}

}
