using System.Collections.Generic;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.General;

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


        private static readonly Regex EnumConstantRegex = new Regex(@"\A\s*(?<enumName>[\w\d_]+)\.(?<enumValue>[\w\d_]+)\s*\Z", RegexOptions.Compiled);

        private static readonly Regex VariableNameRegex = new Regex(@"\A\s*<(?<variableName>[\w\d_]+)>\s*\Z", RegexOptions.Compiled);


        /// <summary>
        /// Deserialize some text as a constant.
        /// </summary>
        public static ProcessMember Deserialize(string text, ProcessFactoryStore processFactoryStore)
        {
            if (EnumConstantRegex.TryMatch(text, out var enumMatch))
            {
                var result = processFactoryStore.EnumTypesDictionary
                    .TryFindOrFail(enumMatch.Groups["enumName"].Value,
                        $"Could not recognize enum '{enumMatch.Groups["enumName"].Value}'")
                    .Bind(x => Extensions.TryGetEnumValue(x, enumMatch.Groups["enumValue"].Value))
                    .Map(x => new ProcessMember(new ConstantFreezableProcess(x)));

                if (result.IsSuccess)
                    return result.Value;
            }

            if (VariableNameRegex.TryMatch(text, out var variableNameMatch))
            {
                var variableName = new VariableName(variableNameMatch.Groups["variableName"].Value);
                var fp = new FreezableProcessData(new Dictionary<string, ProcessMember>
                {
                    {nameof(GetVariable<object>.VariableName), new ProcessMember(variableName)}
                });
                var getValueProcess =new CompoundFreezableProcess(GetVariableProcessFactory.Instance, fp);

                return new ProcessMember(getValueProcess);
            }

            if (bool.TryParse(text, out var b))
                return new ProcessMember(new ConstantFreezableProcess(b));
            if (int.TryParse(text, out var i))
                return new ProcessMember(new ConstantFreezableProcess(i));
            return new ProcessMember(new ConstantFreezableProcess(text));
        }
    }
}