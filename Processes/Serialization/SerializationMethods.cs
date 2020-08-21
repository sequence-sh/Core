using System.Collections.Generic;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Deserializes text to primitive types.
    /// </summary>
    public static class SerializationMethods
    {
        private static readonly Regex EnumConstantRegex = new Regex(@"\A\s*(?<enumName>[\w\d_]+)\.(?<enumValue>[\w\d_]+)\s*\Z", RegexOptions.Compiled);

        private static readonly Regex VariableNameRegex = new Regex(@"\A\s*<(?<variableName>[\w\d_]+)>\s*\Z", RegexOptions.Compiled);

        private static readonly Regex StringRegex = new Regex(@"\A\s*(?:'(?<string>.+?)')|(?:""(?<string>.+?)"")\s*\Z", RegexOptions.Compiled);

        //private static readonly Regex VariableNameAsStringRegex = new Regex(@"\A\s*(?<variableName>[\w\d_]+)\s*\Z");


        /// <summary>
        /// Serialize a constant freezable process.
        /// </summary>
        public static string SerializeConstant(ConstantFreezableProcess cfp, bool quoteString)
        {
            if (cfp.Value.GetType().IsEnum)
                return cfp.Value.GetType().Name + "." + cfp.Value;
            else if (cfp.Value is string s)
                return quoteString? $"'{s}'" : s;
            else return cfp.Value.ToString() ?? "";
        }


        /// <summary>
        /// Deserialize some text to a primitive type.
        /// </summary>
        public static Result<ProcessMember> TryDeserialize(string text, ProcessFactoryStore processFactoryStore)
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
                var dict = new Dictionary<string, ProcessMember>
                {
                    {nameof(GetVariable<object>.VariableName), new ProcessMember(variableName)}
                };


                var fp = new FreezableProcessData(dict);
                var getValueProcess = new CompoundFreezableProcess(GetVariableProcessFactory.Instance, fp, null);

                return new ProcessMember(getValueProcess);
            }

            if (StringRegex.TryMatch(text, out var stringMatch))
            {
                var s = stringMatch.Groups["string"].Value;
                return new ProcessMember(new ConstantFreezableProcess(s));
            }


            if (bool.TryParse(text, out var b))
                return new ProcessMember(new ConstantFreezableProcess(b));
            if (int.TryParse(text, out var i))
                return new ProcessMember(new ConstantFreezableProcess(i));

            return new ProcessMember(new ConstantFreezableProcess(text));

            //if (VariableNameAsStringRegex.TryMatch(text, out var vnMatch))//TODO remove this and fail here
            //{
            //    return new ProcessMember(new ConstantFreezableProcess(text));
            //}

            //return Result.Failure<ProcessMember>($"Could not deserialize '{text}'");
        }
    }
}
