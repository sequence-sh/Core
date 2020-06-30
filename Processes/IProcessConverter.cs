using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// A converter that can convert a process to a different type.
    /// </summary>
    public interface IProcessConverter
    {
        /// <summary>
        /// Tries to convert this process to a process of a different type.
        /// </summary>
        public Result<IImmutableProcess<T>> TryConvert<T>(IImmutableProcess<T> immutableProcess, IProcessSettings processSettings);
    }
}
