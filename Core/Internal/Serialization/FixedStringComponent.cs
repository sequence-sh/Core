using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal.Serialization
{
    /// <summary>
    /// Include a fixed string in serialization.
    /// </summary>
    public class FixedStringComponent : ISerializerBlock//, IDeserializerBlock
    {
        /// <summary>
        /// Creates a new FixedStringComponent
        /// </summary>
        public FixedStringComponent(string value) => Value = value;

        /// <summary>
        /// The fixed value to insert.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc />
        public Result<string> TryGetSegmentText(IReadOnlyDictionary<string, StepProperty> dictionary) => Value;
    }
}