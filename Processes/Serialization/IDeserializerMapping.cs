using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Maps a regex group to a property
    /// </summary>
    public interface IDeserializerMapping
    {
        /// <summary>
        /// The name of the regex group to match.
        /// </summary>
        string GetGroupName(int index);

        /// <summary>
        /// The name of the property to map to.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Try to turn the text of the regex group into a process member.
        /// </summary>
        Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore);
    }
}