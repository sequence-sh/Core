using System.Collections.Generic;

#pragma warning disable 1591 // I think the comments here would be pretty tautological

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Gets the names for various processes.
    /// Looks a bit silly right now but will be important once we start thinking about localization.
    /// </summary>
    public static class ProcessNameHelper
    {
        //TODO support different languages

        public static string GetConditionalName(string ifName, string thenName, string? elseName) =>
            elseName == null? $"If ({ifName}) then ({thenName})" : $"If ({ifName}) then ({thenName}) else ({elseName})";

        public static string GetAssertErrorName(string subProcessName) => $"Assert Fail: {subProcessName}";

        public static string GetChainName(string processName, string? nextName)
        {
            if (string.IsNullOrWhiteSpace(nextName))
                return processName;

            return processName + " then " + nextName;
        }

        public static string GetReadFileName()
        {
            return "Read File";
        }

        public static string GetSequenceName(IEnumerable<string> stepNames) //TODO allow names like "Search and Tag 17 times"
        {
            var r = string.Join(" then ", stepNames);

            return string.IsNullOrWhiteSpace(r) ? GetDoNothingName() : r;
        }

        public static string GetLoopName(string @for, string @do) => $"Foreach in {@for}, {@do}";

        public static string GetCreateDirectoryName() => "Create Directory";
        public static string GetDeleteItemName() => "Delete Item";
        public static string GetDoNothingName() => "Do Nothing";

        public static string GetUnzipName() => "Unzip Item";

        public static string GetRunExternalProcessName() => "Run External Process";

        public static string GetAssertFileContainsProcessName() => "Assert File Contains";

        public static string GetCheckNumberProcessName(string countProcessName) => $"Check result of {countProcessName}";
        public static string GetAssertBoolProcessName(string bProcessName, bool b) => $"Assert {bProcessName} returns {b}";

        public static string GetWriteFileProcessName(string subProcessName) => $"Write file from {subProcessName}";

        public static string GetReturnValueProcessName(string result) => $"Return {result}";

        public static string GetReturnBoolProcessName(bool result) => $"Return {result}";

        public static string GetDelayProcessName(int milliseconds) => $"Delay {milliseconds} milliseconds";

        public static string GetOutputMessageProcessName(string message) => $"Output Message {message}";

        public static string ReturnProcessSettingName(string settingName) => $"Return {settingName}";
    }
}
