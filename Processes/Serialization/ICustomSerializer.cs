using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// A custom process serializer.
    /// </summary>
    public interface ICustomSerializer
    {
        /// <summary>
        /// Serialize this data as a process of this type.
        /// </summary>
        Result<string> TrySerialize(FreezableProcessData data);

        /// <summary>
        /// Try to deserialize this data.
        /// </summary>
        Result<IFreezableProcess> TryDeserialize(string s, ProcessFactoryStore processFactoryStore, RunnableProcessFactory factory);
    }
}