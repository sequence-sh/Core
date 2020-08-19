using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Deserializes a regex group into a Variable Name.
    /// </summary>
    public class VariableNameDeserializerMapping : IDeserializerMapping
    {
        public VariableNameDeserializerMapping(string groupName, string propertyName)
        {
            GroupName = groupName;
            PropertyName = propertyName;
        }

        /// <inheritdoc />
        public string GroupName { get; }

        /// <inheritdoc />
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore) => new ProcessMember(new VariableName(groupText));
    }
}