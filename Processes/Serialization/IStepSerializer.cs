using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// A custom step serializer.
    /// </summary>
    public interface IStepSerializer
    {
        /// <summary>
        /// Serialize this data as a step of this type.
        /// </summary>
        Result<string> TrySerialize(FreezableStepData data);
    }
}