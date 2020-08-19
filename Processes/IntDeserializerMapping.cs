using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Deserializes a regex group into an integer
    /// </summary>
    public class IntDeserializerMapping : IDeserializerMapping
    {
        public IntDeserializerMapping(string groupName, string propertyName)
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
            if (int.TryParse(groupText, out var i))
                return new ProcessMember(new ConstantFreezableProcess(i));
            return Result.Failure<ProcessMember>($"Could not parse '{groupText}' as an integer");


        }
    }
}