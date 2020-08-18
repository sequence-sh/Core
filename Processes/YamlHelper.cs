using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Allows you to get a process factory from a process name.
    /// </summary>
    public class ProcessFactoryStore
    {
        /// <summary>
        /// Creates a new ProcessFactoryStore.
        /// </summary>
        public ProcessFactoryStore(IReadOnlyDictionary<string, RunnableProcessFactory> dictionary, IReadOnlyDictionary<string, Type> enumTypesDictionary)
        {
            Dictionary = dictionary;
            EnumTypesDictionary = enumTypesDictionary;
        }


        /// <summary>
        /// Create a process factory store using all ProcessFactories in the assembly.
        /// </summary>
        /// <returns></returns>
        public static ProcessFactoryStore CreateUsingReflection(Type anyAssemblyMember)
        {
            var factories = Assembly.GetAssembly(anyAssemblyMember)!
                .GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(RunnableProcessFactory).IsAssignableFrom(x))
                .Select(x => x.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)!.GetValue(null))
                .Cast<RunnableProcessFactory>().ToList();

            var dictionary = factories.ToDictionary(x => x.TypeName);
            var enumTypesDictionary = factories.SelectMany(x => x.EnumTypes).Distinct()
                .ToDictionary(x => x.Name ?? "", StringComparer.OrdinalIgnoreCase);

            return new ProcessFactoryStore(dictionary, enumTypesDictionary!);


        }

        /// <summary>
        /// Types of enumerations that can be used by these processes.
        /// </summary>
        public IReadOnlyDictionary<string, Type> EnumTypesDictionary { get; }

        /// <summary>
        /// Dictionary mapping process names to process factories.
        /// </summary>
        public IReadOnlyDictionary<string, RunnableProcessFactory> Dictionary { get; }
    }


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
            //TODO special deserializers

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
                if (EnumConstantRegex.TryMatch(sString3, out var m))
                {
                    result = processFactoryStore.EnumTypesDictionary
                        .TryFindOrFail(m.Groups["enumName"].Value,
                            $"Could not recognize enum '{m.Groups["enumName"].Value}'")
                        .Bind(x => Extensions.TryGetEnumValue(x, m.Groups["enumValue"].Value))
                        .Map(x => new ProcessMember(new ConstantFreezableProcess(x)));
                }
                else if(bool.TryParse(sString3, out var b))
                    result = Result.Success(new ProcessMember(new ConstantFreezableProcess(b)));
                else if (int.TryParse(sString3, out var i))
                    result = Result.Success(new ProcessMember(new ConstantFreezableProcess(i)));
                else
                    result = Result.Success(new ProcessMember(new ConstantFreezableProcess(sString3)));
            }
            else
                throw new ArgumentOutOfRangeException(nameof(simpleObject));

            return result;

            static Result<IFreezableProcess> CreateProcess(RunnableProcessFactory factory,
                            IEnumerable<(string key, ProcessMember member)> arguments)
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
        }

        private static readonly Regex EnumConstantRegex = new Regex(@"(?<enumName>[\w\d_]+)\.(?<enumValue>[\w\d_]+)");

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
