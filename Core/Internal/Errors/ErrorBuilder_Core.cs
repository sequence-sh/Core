using System;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// An error without a location.
/// </summary>
// ReSharper disable once InconsistentNaming
public class ErrorBuilder_Core : SingleErrorBuilder<ErrorCode_Core>
{
    /// <inheritdoc />
    public ErrorBuilder_Core(Exception exception, ErrorCode_Core errorCode) : base(
        errorCode,
        exception
    ) { }

    /// <inheritdoc />
    public ErrorBuilder_Core(ErrorCode_Core errorCode, params object?[] data) : base(
        errorCode,
        data
    ) { }

    /// <inheritdoc />
    public override SingleError WithLocationSingle(IErrorLocation location)
    {
        return Data.Match(
            x => new SingleError_Core(location, x,         ErrorCode),
            x => new SingleError_Core(location, ErrorCode, x)
        );
    }

    /// <inheritdoc />
    public override IErrorCodeHelper<ErrorCode_Core> ErrorCodeHelper =>
        ErrorCodeHelper_Core_EN.Instance;
}

}
