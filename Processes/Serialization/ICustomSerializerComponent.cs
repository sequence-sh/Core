namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// A component of a custom serializer
    /// </summary>
    public interface ICustomSerializerComponent
    {
        /// <summary>
        /// Contributes to the serialized string
        /// </summary>
        public ISerializerBlock? SerializerBlock { get; }

        ///// <summary>
        ///// Contributes to the deserialization regex
        ///// </summary>
        //public IDeserializerBlock? DeserializerBlock { get; }

        //public IDeserializerMapping? Mapping { get; }
    }
}