using System.Collections.Generic;

#pragma warning disable 1591 // I think the comments here would be pretty tautological

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Gets the names for various processes.
    /// Looks a bit silly right now but will be important once we start thinking about localization.
    /// </summary>
    public static class ProcessNameHelper
    {
        //TODO support different languages

        public static string GetConditionalName(string ifName, string thenName, string? elseName)
        {
            return elseName == null? $"If ({ifName}) then ({thenName})" : $"If ({ifName}) then ({thenName}) else ({elseName})";
        }

        public static string GetAssertErrorName(string subProcessName)
        {
            return $"Assert Fail: {subProcessName}";
        }

        public static string GetSequenceName(IEnumerable<string> stepNames)
        {
            var r = string.Join(" then ", stepNames);

            return string.IsNullOrWhiteSpace(r) ? GetDoNothingName() : r;
        }

        public static string GetCreateDirectoryName() => "Create Directory";
        public static string GetDeleteItemName() => "Delete Item";
        public static string GetDoNothingName() => "Do Nothing";

        public static string GetUnzipName() => "Unzip Item";

        public static string GetRunExternalProcessName() => "Run External Process";

        public static string GetAssertFileContainsProcess() => "Assert File Contains";
    }
}
