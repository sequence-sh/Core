using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Include a required space in serialization.
    /// </summary>
    public class SpaceComponent : IStepSerializerComponent, ISerializerBlock//, IDeserializerBlock
    {
        /// <summary>
        /// Create a new Space Component.
        /// </summary>
        public SpaceComponent()
        {

        }


        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableStepData data) => " ";
    }
}