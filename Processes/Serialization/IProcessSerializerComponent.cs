namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// A component of a process serializer
    /// </summary>
    public interface IProcessSerializerComponent
    {
        /// <summary>
        /// Contributes to the serialized string
        /// </summary>
        public ISerializerBlock? SerializerBlock { get; }

    }
}