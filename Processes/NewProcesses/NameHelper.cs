using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// Creates the names for various processes.
    /// </summary>
    public static class NameHelper
    {
        //TODO make all arguments IRunnableProcess

        internal static string GetConstantName(object constantValue) => $"<{constantValue}>";

        internal static string GetGetVariableName(string variableName) => $"[{variableName}]";

        internal static string GetSetVariableName(string variableName, string value) => $"[{variableName}] = '{value}'";

        internal static string GetIfName(IFreezableProcess @if, IFreezableProcess then, IFreezableProcess? @else) => $"Conditional '{@if}' then '{then}' else '{@else}'.";

        internal static string GetTestName(IFreezableProcess @if, IFreezableProcess then, IFreezableProcess? @else) => $"Conditional '{@if}' then '{then}' else '{@else}'.";

        internal static string GetArrayName(int count) => $"[{count}]";

        internal static string GetPrintName(IFreezableProcess value) => $"Print '{value.Name}'";

        internal static string GetCompareName(IFreezableProcess item1, IFreezableProcess @operator, IFreezableProcess item2) => $"{item1.Name} {@operator.Name} {item2.Name}";

        internal static string GetSequenceName(IEnumerable<IFreezableProcess> members)
        {
            var s = string.Join("; ", members.Select(x=>x.Name));
            return string.IsNullOrWhiteSpace(s) ? "Do Nothing" : s;
        }

        public class MissingProcess : IFreezableProcess
        {
            private MissingProcess()
            {
            }

            public static IFreezableProcess Instance { get; } = new MissingProcess();

            ///// <inheritdoc />
            //public string Name => "Unknown";

            ///// <inheritdoc />
            //public IFreezableProcess Unfreeze()
            //{
            //    throw new System.NotImplementedException();
            //}
            /// <inheritdoc />
            public Result<IRunnableProcess> TryFreeze(ProcessContext processContext)
            {
                throw new System.NotImplementedException();
            }

            /// <inheritdoc />
            public Result<IReadOnlyCollection<(string name, ITypeReference type)>> TryGetVariablesSet { get; }

            /// <inheritdoc />
            public string Name => "Unknown";

            /// <inheritdoc />
            public Result<ITypeReference> TryGetOutputTypeReference()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}