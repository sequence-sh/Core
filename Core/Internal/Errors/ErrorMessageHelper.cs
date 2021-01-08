using System;
using System.Diagnostics;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// Localizes core error messages
/// </summary>
// ReSharper disable once InconsistentNaming
public sealed class ErrorCodeHelper_Core_EN : IErrorCodeHelper<ErrorCode_Core>
{
    private ErrorCodeHelper_Core_EN() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static IErrorCodeHelper<ErrorCode_Core> Instance { get; } =
        new ErrorCodeHelper_Core_EN();

    /// <inheritdoc />
    public string GetFormatString(ErrorCode_Core code)
    {
        var localizedMessage = ErrorMessages_EN.ResourceManager.GetString(code.ToString());
        Debug.Assert(localizedMessage != null, nameof(localizedMessage) + " != null");

        return localizedMessage;
    }
}

/// <summary>
/// Supports localization of error messages.
/// </summary>
public interface IErrorCodeHelper<in TCode>
    where TCode : Enum
{
    /// <summary>
    /// Gets the format string for this error
    /// </summary>
    public string GetFormatString(TCode code);

    /// <summary>
    /// Gets a formatted localized message for an error code
    /// </summary>
    string GetFormattedMessage(
        TCode code,
        params object?[] args)
    {
        var message          = GetFormatString(code);
        var formattedMessage = string.Format(message, args);

        return formattedMessage;
    }
}

}
