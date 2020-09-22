using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// A custom process serializer.
    /// </summary>
    public interface IProcessSerializer
    {
        /// <summary>
        /// Serialize this data as a process of this type.
        /// </summary>
        Result<string> TrySerialize(FreezableProcessData data);
    }
}