using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// A process that gets the value of a particular variable.
    /// </summary>
    public sealed class GetVariableFreezableProcess : IFreezableProcess
    {
        /// <summary>
        /// Creates a new GetVariableFreezableProcess.
        /// </summary>
        /// <param name="variableName"></param>
        public GetVariableFreezableProcess(string variableName)
        {
            VariableName = variableName;
        }

        /// <summary>
        /// The name of the variable to get
        /// </summary>
        public string VariableName { get; }


        /// <inheritdoc />
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext)
        {
            if (!processContext.VariableTypesDictionary.TryGetValue(VariableName, out var type))
                return Result.Failure<IRunnableProcess>($"The variable '{VariableName}' is never set.");

            Type processType = typeof(GetVariableRunnableProcess<>).MakeGenericType(type);
            var process = Activator.CreateInstance(processType, VariableName);

            //TODO check for exceptions here?

            var runnableProcess = (IRunnableProcess)process!;

            return Result.Success(runnableProcess);
        }

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(string name, ITypeReference type)>> TryGetVariablesSet => ImmutableList<(string name, ITypeReference type)>.Empty;


        /// <inheritdoc />
        public string ProcessName => NameHelper.GetGetVariableName(VariableName);

        /// <inheritdoc />
        public Result<ITypeReference> TryGetOutputTypeReference() => new VariableTypeReference(VariableName);

        /// <inheritdoc />
        public override string ToString() => ProcessName;
    }
}