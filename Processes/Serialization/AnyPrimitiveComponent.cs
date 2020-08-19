using System.Collections.Generic;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.General;

namespace Reductech.EDR.Processes
{



    /// <summary>
    /// Deserializes a regex group into a constant of any type.
    /// </summary>
    public class AnyPrimitiveComponent : IDeserializerMapping, ISerializerBlock, IDeserializerBlock, ICustomSerializerComponent
    {
        public AnyPrimitiveComponent(string propertyName) => PropertyName = propertyName;

        /// <inheritdoc />
        public string GetGroupName(int index) => "Value" + index;

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

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableProcessData data) =>
            data.Dictionary
                .TryFindOrFail(PropertyName, null)
                .Bind(x => x.Join<Result<string>>(VariableNameComponent.Serialize,
                    TrySerialize,
                    _ => Result.Failure<string>("Cannot serialize list")

                ));

        private static Result<string> TrySerialize(IFreezableProcess process)
        {
            if (process is ConstantFreezableProcess constantFreezableProcess)
            {
                return SerializeConstant(constantFreezableProcess);
            }
            else if (process is CompoundFreezableProcess compound && compound.ProcessFactory == GetVariableProcessFactory.Instance) //Special case
            {
                return compound.SerializeToYaml().Trim();
            }

            return Result.Failure<string>("Cannot serialize compound as a primitive");
        }


        /// <summary>
        /// Serialize a constant freezable process.
        /// </summary>
        /// <param name="cfp"></param>
        /// <returns></returns>
        public static string SerializeConstant(ConstantFreezableProcess cfp)
        {
            if (cfp.Value.GetType().IsEnum)
            {
                return cfp.Value.GetType().Name + "." + cfp.Value;
            }
            else if (cfp.Value is string s)
                return $"'{s}'";
            else return cfp.Value.ToString() ?? "";
        }


        /// <inheritdoc />
        public string GetRegexText(int index) => @$"(?:(?<{GetGroupName(index)}>(?:[\w\d\._]+))|'(?<{GetGroupName(index)}>.+?)'|(?<{GetGroupName(index)}><[\w\d\._]+?>))";

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        /// <inheritdoc />
        public IDeserializerBlock? DeserializerBlock  => this;

        /// <inheritdoc />
        public IDeserializerMapping? Mapping => this;
    }
}