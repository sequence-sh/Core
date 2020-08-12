//using CSharpFunctionalExtensions;

//namespace Reductech.EDR.Processes.NewProcesses
//{
//    /// <summary>
//    /// A process that gets a particular variable.
//    /// </summary>
//    public sealed class GetVariableRunnableProcess<T> : IRunnableProcess<T>
//    {
//        /// <summary>
//        /// Creates a new GetVariableRunnableProcess.
//        /// </summary>
//        /// <param name="variableName"></param>
//        public GetVariableRunnableProcess(string variableName) => VariableName = variableName;

//        /// <summary>
//        /// The name of the variable.
//        /// </summary>
//        public string VariableName { get; }


//        /// <inheritdoc />
//        public Result<T> Run(ProcessState processState) => processState.GetVariable<T>(VariableName);

//        /// <inheritdoc />
//        public string Name => NameHelper.GetGetVariableName(VariableName);

//        /// <inheritdoc />
//        public IFreezableProcess Unfreeze() => new GetVariableFreezableProcess(VariableName);

//        /// <inheritdoc />
//        public override string ToString() => Name;
//    }
//}