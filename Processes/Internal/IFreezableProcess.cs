using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A process which can be Runnable.
    /// </summary>
    public interface IFreezableProcess
    {
        /// <summary>
        /// Try to freeze this process.
        /// </summary>
        Result<IRunnableProcess> TryFreeze(ProcessContext processContext);

        /// <summary>
        /// Gets the variables which may be set by this process and their types.
        /// </summary>
        Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference type)>> TryGetVariablesSet { get; }


        /// <summary>
        /// The human-readable name of this process.
        /// </summary>
        string ProcessName { get; }


        /// <summary>
        /// The output type of this process. Will be unit if the process does not have an output.
        /// </summary>
        Result<ITypeReference> TryGetOutputTypeReference();
    }

    /// <summary>
    /// SerializationHelper methods for processes.
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Tries to freeze this process.
        /// </summary>
        public static Result<IRunnableProcess> TryFreeze(this IFreezableProcess process) =>
            ProcessContext.TryCreate(process)
                .Bind(process.TryFreeze);


    }
}