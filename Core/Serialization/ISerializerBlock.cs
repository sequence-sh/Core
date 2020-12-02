using System.Collections.Generic;
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
        /// Gets the segment of serialized text if possible
        /// </summary>
        public Result<string> TryGetSegmentText(IReadOnlyDictionary<string, StepProperty> dictionary);

    }
}