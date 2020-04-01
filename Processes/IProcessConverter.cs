using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// A converter that can convert a process to a different type.
    /// </summary>
    public interface IProcessConverter
    {
        /// <summary>
        /// Tries to convert this process to a process of a different type.
        /// </summary>
        public Result<ImmutableProcess<T>> TryConvert<T>(ImmutableProcess<T> immutableProcess, IProcessSettings processSettings);
    }
}
