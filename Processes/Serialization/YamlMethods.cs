using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Contains methods for converting Processes to and from Yaml.
    /// </summary>
    public static class YamlMethods
    {
        /// <summary>
        /// Serialize this process to yaml.
        /// </summary>
        public static string SerializeToYaml(this IFreezableProcess process)
        {
            var obj = SimplifyProcess(process, true);
            var serializer = new YamlDotNet.Serialization.Serializer();

            var r = serializer.Serialize(obj).Trim();

            return r;
        }

        /// <summary>
        /// Deserialize this yaml into a process.
        /// </summary>
        public static Result<IFreezableProcess > DeserializeFromYaml(string yaml, ProcessFactoryStore processFactoryStore)
        {
            var deserializer = new YamlDotNet.Serialization.Deserializer();

            var o = deserializer.Deserialize<object>(yaml);

            var parser = new ProcessMemberParser(processFactoryStore);

            var result = FromSimpleObject(o, parser);

            return result.Bind(x =>
                x.Join(vn => Result.Failure<IFreezableProcess>("Yaml must contain a process or list of processes"),
                        Result.Success,
                        l => new CompoundFreezableProcess(SequenceProcessFactory.Instance,
                            new FreezableProcessData(new Dictionary<string, ProcessMember>
                                {{nameof(Sequence.Steps), new ProcessMember(l)}}), null)

                    ));
        }

        private const string TypeString = "Do";
        private const string ConfigString = "Config";

        private static Result<ProcessMember> FromSimpleObject(object simpleObject, ProcessMemberParser processMemberParser)
        {

            Result<ProcessMember> result;

            switch (simpleObject)
            {
                case List<object> list:
                    result = list.Select(x => FromSimpleObject(x, processMemberParser))
                        .Select(x => x.Bind(y => y.AsArgument("Array Member")))
                        .Combine()
                        .Map(x => new ProcessMember(x.ToList()));
                    break;
                case Dictionary<object, object> dictionary1 when dictionary1.ContainsKey(TypeString):
                {
                    var processConfiguration = dictionary1
                        .TryFindOrFail(ConfigString, null)
                        .Bind(ProcessConfiguration.TryConvert)
                        .OnFailureCompensate(x=> (null as ProcessConfiguration)!);

                    result = dictionary1.TryFindOrFail(TypeString, $"Object did not have {TypeString} set.")
                        .BindCast<object, string>()
                        .Bind(x => processMemberParser.ProcessFactoryStore.Dictionary.TryFindOrFail(x, $"Could not find the process: '{x}'."))
                        .Compose(() =>
                            dictionary1.Where(x => x.Key.ToString() != TypeString && x.Key.ToString() != ConfigString)
                                .Select(x =>
                                    FromSimpleObject(x.Value, processMemberParser)
                                        .Map(value => (x.Key.ToString(), value)))
                                .Combine())
                        .Bind(x => CreateProcess(x.Item1, x.Item2!, processConfiguration.Value))
                        .Map(x => new ProcessMember(x));
                    break;
                }
                case string sString3:
                    result = processMemberParser.TryParse(sString3);

                    if (result.IsFailure)
                        return new ProcessMember(new ConstantFreezableProcess(sString3));

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(simpleObject));
            }

            return result;

            static Result<IFreezableProcess> CreateProcess(RunnableProcessFactory factory, IEnumerable<(string key, ProcessMember member)> arguments, ProcessConfiguration? processConfiguration)
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
                    else if (expectedMemberType == MemberType.Process && memberType == MemberType.VariableName)
                    {
                        var freezableProcess = GetVariableProcessFactory.CreateFreezable(value.VariableName!.Value);
                        dict.Add(key, new ProcessMember(freezableProcess));
                    }
                    else
                        errors.Add($"'{key}' has the wrong MemberType in {factory.TypeName}");
                }

                if (errors.Any())
                    return errors.Select(Result.Failure).Combine().ConvertFailure<IFreezableProcess>();

                var data = new FreezableProcessData(dict);

                var process = new CompoundFreezableProcess(factory, data, processConfiguration);
                return process;
            }
        }

        private static object SimplifyProcess(IFreezableProcess process, bool isTopLevel)
        {
            switch (process)
            {
                case ConstantFreezableProcess cfp:
                    return SerializationMethods.SerializeConstant(cfp, false);
                case CompoundFreezableProcess compoundFreezableProcess:
                {
                    if (isTopLevel && compoundFreezableProcess.ProcessFactory == SequenceProcessFactory.Instance &&
                        compoundFreezableProcess.FreezableProcessData.Dictionary.TryGetValue(nameof(Sequence.Steps), out var processMember))
                        return ToSimpleObject(processMember);

                    if (compoundFreezableProcess.ProcessConfiguration == null)//Don't use custom serialization if you have configuration
                    {
                            var sr = compoundFreezableProcess.ProcessFactory.Serializer
                                    .TrySerialize(compoundFreezableProcess.FreezableProcessData);
                            if (sr.IsSuccess) //Serialization will not always succeed.
                                return sr.Value;

                    }

                    IDictionary<string, object> expandoObject = new ExpandoObject();
                    expandoObject[TypeString] = compoundFreezableProcess.ProcessFactory.TypeName;

                    if (compoundFreezableProcess.ProcessConfiguration != null)
                        expandoObject[ConfigString] = compoundFreezableProcess.ProcessConfiguration;

                    foreach (var (name, m) in compoundFreezableProcess.FreezableProcessData.Dictionary)
                        expandoObject[name] = ToSimpleObject(m);

                    return expandoObject;

                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(process));
            }
        }


        private static object ToSimpleObject(ProcessMember member) =>
            member.Join(x=> VariableNameComponent.Serialize(x).Value,
                x=> SimplifyProcess(x, false),
                l=>l.Select(x=>SimplifyProcess(x, false)).ToList());
    }
}
