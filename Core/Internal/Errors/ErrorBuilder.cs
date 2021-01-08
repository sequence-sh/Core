using System;
using OneOf;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// A generic single error builder
/// </summary>
public abstract class SingleErrorBuilder<TCode> : SingleErrorBuilder where TCode : Enum
{
    /// <inheritdoc />
    protected SingleErrorBuilder(TCode errorCode, OneOf<Exception, object?[]> data)
    {
        ErrorCode = errorCode;
        Data      = data;
    }

    /// <summary>
    /// The error code
    /// </summary>
    public TCode ErrorCode { get; }

    /// <summary>
    /// The data
    /// </summary>
    protected OneOf<Exception, object?[]> Data { get; }

    /// <inheritdoc />
    public override string ToString() => AsString;

    /// <inheritdoc />
    public override string AsString => Data.Match(
        x => x.Message,
        x => ErrorCodeHelper.GetFormattedMessage(ErrorCode, x)
    );

    /// <summary>
    /// The Error Code helper
    /// </summary>
    public abstract IErrorCodeHelper<TCode> ErrorCodeHelper { get; }
}

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
