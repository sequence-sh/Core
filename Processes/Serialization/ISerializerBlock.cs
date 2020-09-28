using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Contributes to the serialized string
    /// </summary>
    public interface ISerializerBlock
    {
        /// <summary>
        /// Gets the segment of serialized text.
        /// </summary>
        public Result<string> TryGetText(FreezableStepData data);
    }
}