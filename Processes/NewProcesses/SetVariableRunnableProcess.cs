using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// Sets the value of a particular variable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SetVariableRunnableProcess<T> : IRunnableProcess<Unit>
    {
        /// <summary>
        /// Creates a new SetVariableRunnableProcess.
        /// </summary>
        public SetVariableRunnableProcess(string variableName, IRunnableProcess<T> value)
        {
            VariableName = variableName;
            Value = value;
        }

        /// <inheritdoc />
        public Result<Unit> Run(ProcessState processState)
        {
            var valueResult = Value.Run(processState);

            if (valueResult.IsFailure)
                return valueResult.ConvertFailure<Unit>();

            processState.SetVariable(VariableName, valueResult.Value);

            return Unit.Default;
        }

        /// <inheritdoc />
        public string Name => NameHelper.GetSetVariableName(VariableName, Value.Name);

        /// <summary>
        /// The name of the variable to set.
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// The value that the variable will be set to.
        /// </summary>
        public IRunnableProcess<T> Value { get; }


        /// <inheritdoc />
        public IFreezableProcess Unfreeze() => new SetVariableFreezableProcess(VariableName, Value.Unfreeze());


        /// <inheritdoc />
        public override string ToString() => Name;
    }
}