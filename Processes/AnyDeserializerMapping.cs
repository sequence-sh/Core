using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Deserializes a regex group into a constant of any type.
    /// </summary>
    public class AnyDeserializerMapping : IDeserializerMapping
    {
        public AnyDeserializerMapping(string groupName, string propertyName)
        {
            GroupName = groupName;
            PropertyName = propertyName;
        }

        /// <inheritdoc />
        public string GroupName { get; }

        /// <inheritdoc />
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore) => Deserialize(groupText, processFactoryStore);


        private static readonly Regex EnumConstantRegex = new Regex(@"(?<enumName>[\w\d_]+)\.(?<enumValue>[\w\d_]+)");


        /// <summary>
        /// Deserialize some text as a constant.
        /// </summary>
        public static ProcessMember Deserialize(string text, ProcessFactoryStore processFactoryStore)
        {
            if (EnumConstantRegex.TryMatch(text, out var m))
            {
                var result = processFactoryStore.EnumTypesDictionary
                    .TryFindOrFail(m.Groups["enumName"].Value,
                        $"Could not recognize enum '{m.Groups["enumName"].Value}'")
                    .Bind(x => Extensions.TryGetEnumValue(x, m.Groups["enumValue"].Value))
                    .Map(x => new ProcessMember(new ConstantFreezableProcess(x)));

                if (result.IsSuccess)
                    return result.Value;
            }

            if (bool.TryParse(text, out var b))
                return new ProcessMember(new ConstantFreezableProcess(b));
            if (int.TryParse(text, out var i))
                return new ProcessMember(new ConstantFreezableProcess(i));
            return new ProcessMember(new ConstantFreezableProcess(text));
        }
    }
}