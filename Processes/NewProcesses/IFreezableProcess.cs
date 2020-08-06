using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
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
        Result<IReadOnlyCollection<(string name, ITypeReference type)>> TryGetVariablesSet { get; }


        /// <summary>
        /// The human-readable name of this process.
        /// </summary>
        string Name { get; }


        /// <summary>
        /// The output type of this process. Will be unit if the process does not have an output.
        /// </summary>
        Result<ITypeReference> TryGetOutputTypeReference();

        //TODO serialize

        //TODO name
    }
}