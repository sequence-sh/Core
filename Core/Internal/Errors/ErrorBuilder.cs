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
    protected SingleErrorBuilder(TCode errorCode, OneOf<Exception, object?[]> data) : base(data) =>
        ErrorCode = errorCode;

    /// <summary>
    /// The error code
    /// </summary>
    public TCode ErrorCode { get; }

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

}
