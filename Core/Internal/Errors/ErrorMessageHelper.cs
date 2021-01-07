using System.Diagnostics;

namespace Reductech.EDR.Core.Internal.Errors
{
    internal static class ErrorMessageHelper
    {
        public static string GetFormattedMessage(this ErrorCode code, params object?[] args)
        {
            var localizedMessage = ErrorMessages_EN.ResourceManager.GetString(code.ToString());

            Debug.Assert(localizedMessage is not null, nameof(localizedMessage) + " != null");
            var formattedMessage = string.Format(localizedMessage, args);

            return formattedMessage;
        }

    }
}
