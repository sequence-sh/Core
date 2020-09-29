using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Serialization
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