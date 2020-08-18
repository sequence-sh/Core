using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Contains methods for converting Processes to and from Yaml.
    /// </summary>
    public static class YamlHelper
    {
        /// <summary>
        /// Serialize this process to yaml.
        /// </summary>
        public static string SerializeToYaml(this IFreezableProcess process)
        {
            var obj = SimplifyProcess(process);
            var serializer = new YamlDotNet.Serialization.Serializer();

            var r = serializer.Serialize(obj);

            return r;
        }

        /// <summary>
        /// Deserialize this yaml into a process.
        /// </summary>
        public static Result<IFreezableProcess> DeserializeFromYaml(string yaml, ProcessFactoryStore processFactoryStore)
        {
            var deserializer = new YamlDotNet.Serialization.Deserializer();

            var o = deserializer.Deserialize<object>(yaml);

            var result = FromSimpleObject(o, processFactoryStore);

            return result.Bind(x => x.AsArgument("Process"));
        }

        private const string TypeString = "Do";


        private static Result<ProcessMember> FromSimpleObject(object simpleObject, ProcessFactoryStore processFactoryStore)
        {

            Result<ProcessMember> result;

            if (simpleObject is List<object> list)
            {
                result = list.Select(x => FromSimpleObject(x, processFactoryStore))
                    .Select(x => x.Bind(y => y.AsArgument("Array Member")))
                    .Combine()
                    .Map(x => new ProcessMember(x.ToList()));
            }
            else if (simpleObject is Dictionary<object, object> dictionary1 && dictionary1.ContainsKey(TypeString))
            {
                result = dictionary1.TryFindOrFail(TypeString, $"Object did not have {TypeString} set.")
                    .BindCast<object, string>()
                    .Bind(x => processFactoryStore.Dictionary.TryFindOrFail(x, $"Could not find the process: '{x}'."))
                    .Compose(() =>
                        dictionary1.Where(x => x.Key.ToString() != TypeString)
                            .Select(x =>
                                FromSimpleObject(x.Value, processFactoryStore)
                                    .Map(value => (x.Key.ToString(), value)))
                            .Combine())
                    .Bind(x => CreateProcess(x.Item1, x.Item2!))
                    .Map(x => new ProcessMember(x));
            }
            else if (simpleObject is string sString3)
            {
                var special = TrySpecialDeserialize(sString3, processFactoryStore);

                if (special != null)
                    result = special;
                else
                    result = AnyDeserializerMapping.Deserialize(sString3, processFactoryStore);
            }
            else
                throw new ArgumentOutOfRangeException(nameof(simpleObject));

            return result;

            static Result<IFreezableProcess> CreateProcess(RunnableProcessFactory factory, IEnumerable<(string key, ProcessMember member)> arguments)
            {
                var errors = new List<string>();

                var dict = new Dictionary<string, ProcessMember>();

                foreach (var (key, value) in arguments)
                {
                    var expectedMemberType = factory.GetExpectedMemberType(key);
                    var memberType = value.MemberType;

                    if (expectedMemberType == MemberType.NotAMember)
                        errors.Add($"'{key}' is not a member of type {factory.TypeName}");
                    else if (memberType == expectedMemberType)
                        dict.Add(key, value);
                    else if (expectedMemberType == MemberType.VariableName && memberType == MemberType.Process)
                    {
                        //Weird special case - convert this process to a variable name
                        var newValue = value.AsArgument(key)
                            .BindCast<IFreezableProcess, ConstantFreezableProcess>()
                            .Map(x => new VariableName(x.Value.ToString()!))
                            .Map(x => new ProcessMember(x));
                        if (newValue.IsFailure)
                            errors.Add(newValue.Error);
                        else
                            dict.Add(key, newValue.Value);
                    }
                    else
                        errors.Add($"'{key}' has the wrong type in {factory.TypeName}");
                }

                if (errors.Any())
                    return errors.Select(Result.Failure).Combine().ConvertFailure<IFreezableProcess>();

                var data = new FreezableProcessData(dict);

                var process = new CompoundFreezableProcess(factory, data);
                return process;
            }

            static ProcessMember? TrySpecialDeserialize(string s, ProcessFactoryStore processFactoryStore)
            {
                foreach (var customSerializer in processFactoryStore.Dictionary.Values.Select(x=>x.CustomSerializer))
                {
                    if (customSerializer == null) continue;
                    var r = customSerializer.TryDeserialize(s, processFactoryStore);

                    if (r.IsSuccess)
                        return new ProcessMember(r.Value);
                }

                return null;
            }
        }

        private static object SimplifyProcess(IFreezableProcess process)
        {
            switch (process)
            {
                case ConstantFreezableProcess constantFreezableProcess when constantFreezableProcess.Value.GetType().IsEnum:
                    return constantFreezableProcess.Value.GetType().Name + "." + constantFreezableProcess.Value;
                case ConstantFreezableProcess constantFreezableProcess:
                    return constantFreezableProcess.Value.ToString() ?? "";
                case CompoundFreezableProcess compoundFreezableProcess:
                {
                    var customSerializer = compoundFreezableProcess.ProcessFactory.CustomSerializer;

                    if (customSerializer != null)
                        return customSerializer.Serialize(compoundFreezableProcess.FreezableProcessData);


                    IDictionary<string, object> expandoObject = new ExpandoObject();
                    expandoObject[TypeString] = compoundFreezableProcess.ProcessFactory.TypeName;

                    foreach (var (name, m) in compoundFreezableProcess.FreezableProcessData.Dictionary)
                        expandoObject[name] = ToSimpleObject(m);

                    return expandoObject;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(process));
            }
        }


        private static object ToSimpleObject(ProcessMember member) =>
            member.Join(x=>x.Name, SimplifyProcess, l=>l.Select(SimplifyProcess).ToList());
    }
}
