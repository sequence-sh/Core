using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// Sets the value of a particular variable.
    /// </summary>
    public sealed class SetVariableFreezableProcess : IFreezableProcess
    {
        /// <summary>
        /// Creates a new SetVariableFreezableProcess.
        /// </summary>
        public SetVariableFreezableProcess(string variableName, IFreezableProcess value)
        {
            VariableName = variableName;
            Value = value;
        }

        /// <inheritdoc />
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext)
        {
            var valueFreezeResult = Value.TryFreeze(processContext);

            if (valueFreezeResult.IsFailure)
                return valueFreezeResult.ConvertFailure<IRunnableProcess>();

            var outputTypeResult = Value.TryGetOutputTypeReference().Bind(processContext.TryGetTypeFromReference);

            if (outputTypeResult.IsFailure)
                return outputTypeResult.ConvertFailure<IRunnableProcess>();


            Type processType = typeof(SetVariableRunnableProcess<>).MakeGenericType(outputTypeResult.Value);

            var frozenValue = valueFreezeResult.Value;

            var process = Activator.CreateInstance(processType, VariableName, frozenValue);

            //TODO check for exceptions here?

            var runnableProcess = (IRunnableProcess)process!;

            return Result.Success(runnableProcess);
        }

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(string name, ITypeReference type)>> TryGetVariablesSet
        {
            get
            {
                var result = Value.TryGetOutputTypeReference().Map(x => (VariableName, x)).Map(x => new[] {x} as IReadOnlyCollection<(string name, ITypeReference type)>);

                return result;
            }
        }

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// The new value for the variable.
        /// </summary>
        public IFreezableProcess Value { get; }

        /// <inheritdoc />
        public string ProcessName => NameHelper.GetSetVariableName(VariableName, Value.ProcessName);

        /// <inheritdoc />
        public Result<ITypeReference> TryGetOutputTypeReference() => new ActualTypeReference(typeof(Unit));


        /// <inheritdoc />
        public override string ToString() => ProcessName;
    }
}