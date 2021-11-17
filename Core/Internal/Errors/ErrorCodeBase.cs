using System;
using System.Diagnostics.Contracts;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// Identifying code for an error message.
/// </summary>
public abstract record ErrorCodeBase(string Code)
{
    /// <summary>
    /// Get the format string for this Error Code
    /// </summary>
    public abstract string GetFormatString();

    /// <summary>
    /// Gets a formatted localized message for an error code
    /// </summary>
    public string GetFormattedMessage(params object?[] args)
    {
        var message          = GetFormatString();
        var formattedMessage = string.Format(message, args);

        return formattedMessage;
    }

    /// <summary>
    /// The Error Code
    /// </summary>
    public string Code { get; init; } = Code;

    /// <summary>
    /// Create an errorBuilder from this errorCode and some data.
    /// </summary>
    [Pure]
    public ErrorBuilder ToErrorBuilder(params object[] data) => new(this, data);

    /// <summary>
    /// Create an errorBuilder from this errorCode and an exception
    /// </summary>
    [Pure]
    public ErrorBuilder ToErrorBuilder(Exception exception) => new(exception, this);
}

}
