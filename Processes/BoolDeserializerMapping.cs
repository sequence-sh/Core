using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Deserializes a regex group into a boolean.
    /// </summary>
    public class BoolDeserializerMapping : IDeserializerMapping
    {
        public BoolDeserializerMapping(string groupName, string propertyName)
        {
            GroupName = groupName;
            PropertyName = propertyName;
        }

        /// <inheritdoc />
        public string GroupName { get; }

        /// <inheritdoc />
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore)
        {
            if (bool.TryParse(groupText, out var b))
                return new ProcessMember(new ConstantFreezableProcess(b));
            return Result.Failure<ProcessMember>($"Could not parse '{groupText}' as a bool");

        }
    }
}