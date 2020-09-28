using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Include a fixed string in serialization.
    /// </summary>
    public class FixedStringComponent : IProcessSerializerComponent, ISerializerBlock//, IDeserializerBlock
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
        public ISerializerBlock? SerializerBlock => this;

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableStepData data) => Value;
    }
}