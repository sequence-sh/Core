using System;
using JetBrains.Annotations;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// A single error caused by something in Core
/// </summary>
// ReSharper disable once InconsistentNaming
public class SingleError_Core : SingleError<ErrorCode_Core>
{
    /// <inheritdoc />
    public override IErrorCodeHelper<ErrorCode_Core> ErrorCodeHelper =>
        ErrorCodeHelper_Core_EN.Instance;

    /// <inheritdoc />
    public SingleError_Core(
        [NotNull] IErrorLocation location,
        [NotNull] Exception exception,
        ErrorCode_Core errorCode) : base(location, new ErrorBuilder_Core(exception, errorCode)) { }

    /// <inheritdoc />
    public SingleError_Core(
        [NotNull] IErrorLocation location,
        ErrorCode_Core errorCode,
        [NotNull][ItemCanBeNull] params object?[] args) : base(
        location,
        new ErrorBuilder_Core(errorCode, args)
    ) { }
}

}
