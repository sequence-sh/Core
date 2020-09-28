using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Include a required space in serialization.
    /// </summary>
    public class SpaceComponent : IProcessSerializerComponent, ISerializerBlock//, IDeserializerBlock
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