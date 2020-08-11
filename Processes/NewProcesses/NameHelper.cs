using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// Builds the name for a particular instance of a process.
    /// </summary>
    public class ProcessNameBuilder
    {
        /// <summary>
        /// Create a new process name.
        /// </summary>
        /// <param name="templateString"></param>
        public ProcessNameBuilder(string templateString) => TemplateString = templateString;


        /// <summary>
        /// The template to use for this process. Use square brackets for parameters and angle brackets for list parameters.
        /// </summary>
        public string TemplateString { get; }

        /// <summary>
        /// Optional delimiter to use for list properties.
        /// </summary>
        public string ListDelimiter { get; set; } = "; ";

        /// <summary>
        /// Gets the name of the process from the process arguments
        /// </summary>
        public string GetFromArguments(IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            var replacedString = NameVariableRegex.Replace(TemplateString, GetReplacement);

            return replacedString;

            string GetReplacement(Match m)
            {
                if (m.Groups["VariableName"].Success)
                {
                    var variableName = m.Groups["VariableName"].Value;

                    var p = processArguments.TryFind(variableName)
                        .Unwrap(MissingProcess.Instance);

                    return p.ProcessName;
                }
                else
                {
                    var variableName = m.Groups["SequenceVariableName"].Value;

                    var p = processListArguments.TryFind(variableName)
                        .Unwrap(ImmutableArray<IFreezableProcess>.Empty);

                    var s = string.Join(ListDelimiter, p.Select(x => x.ProcessName));

                    return s;
                }
            }

        }



        private static readonly Regex NameVariableRegex = new Regex(@"(\[(?<VariableName>[\w_][\w\d_]*)\])|(\<(?<SequenceVariableName>[\w_][\w\d_]*)\>)", RegexOptions.Compiled);

        private class MissingProcess : IFreezableProcess
        {
            private MissingProcess() { }

            public static IFreezableProcess Instance { get; } = new MissingProcess();
            /// <inheritdoc />
            public Result<IRunnableProcess> TryFreeze(ProcessContext processContext)
            {
                throw new System.NotImplementedException();
            }

            /// <inheritdoc />
            public Result<IReadOnlyCollection<(string name, ITypeReference type)>> TryGetVariablesSet { get; }

            /// <inheritdoc />
            public string ProcessName => "Unknown";

            /// <inheritdoc />
            public Result<ITypeReference> TryGetOutputTypeReference()
            {
                throw new System.NotImplementedException();
            }
        }
    }


    /// <summary>
    /// Creates the names for various processes.
    /// </summary>
    public static class NameHelper
    {
        //TODO make all arguments IRunnableProcess

        internal static string GetConstantName(object constantValue) => $"{constantValue}";

        internal static string GetGetVariableName(string variableName) => $"<{variableName}>";

        internal static string GetSetVariableName(string variableName, object value) => $"<{variableName}> = {GetConstantName(value)}";

        //internal static string GetIfName(IFreezableProcess @if, IFreezableProcess then, IFreezableProcess? @else) => $"Conditional '{@if}' then '{then}' else '{@else}'.";

        //internal static string GetTestName(IFreezableProcess @if, IFreezableProcess then, IFreezableProcess? @else) => $"Conditional '{@if}' then '{then}' else '{@else}'.";

        //internal static string GetArrayName(int count) => $"[{count}]";

        //internal static string GetPrintName(IFreezableProcess value) => $"Print '{value.ProcessName}'";

        //internal static string GetCompareName(IFreezableProcess item1, IFreezableProcess @operator, IFreezableProcess item2) => $"{item1.ProcessName} {@operator.ProcessName} {item2.ProcessName}";

        //internal static string GetSequenceName(IEnumerable<IFreezableProcess> members)
        //{
        //    var s = string.Join("; ", members.Select(x=>x.ProcessName));
        //    return string.IsNullOrWhiteSpace(s) ? "Do Nothing" : s;
        //}

        //internal static string GetNotName(IFreezableProcess boolean) => $"Not {boolean.ProcessName}";

        //internal static string GetAndName(IFreezableProcess left, IFreezableProcess right)=> $"{left.ProcessName} && {right.ProcessName}";
        //internal static string GetOrName(IFreezableProcess left, IFreezableProcess right)=> $"{left.ProcessName} || {right.ProcessName}";

        //internal static string GetRepeatName(IFreezableProcess number, IFreezableProcess process) => $"{number.ProcessName} times, do '{process.ProcessName}'";



    }
}