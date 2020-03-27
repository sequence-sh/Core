using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Base class of all processes.
    /// </summary>
    public abstract class Process
    {
        /// <summary>
        /// The type of this process, or a description of how the type is calculated.
        /// </summary>
        public abstract string GetReturnTypeInfo();

        /// <summary>
        /// The name of this process
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// Executes this process. Should only be called if all conditions are met
        /// </summary>
        /// <returns></returns>
        public abstract Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings);

        /// <summary>
        /// String representation of this process
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetName();
        }
    }
}