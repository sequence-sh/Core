using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using kalexi.Monads.Either.Code;

namespace Reductech.EDR.Processes.NewProcesses
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
        public static ProcessFactoryStore CreateUsingReflection()
        {
            var factories = Assembly.GetAssembly(typeof(RunnableProcessFactory))!
                .GetTypes()
                .Where(x=>!x.IsAbstract)
                .Where(x => typeof(RunnableProcessFactory).IsAssignableFrom(x))
                .Select(x=>x.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)!.GetValue(null))
                .Cast<RunnableProcessFactory>().ToList();

            var dictionary = factories.ToDictionary(x => x.TypeName);
            var enumTypesDictionary = factories.SelectMany(x => x.EnumTypes).Distinct().ToDictionary(x => x.Name!, StringComparer.OrdinalIgnoreCase);

            return new ProcessFactoryStore(dictionary, enumTypesDictionary);


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
            var obj = ToSimpleObject(process);
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

            return result.Bind(x => x.Join(Result.Success,
                r => Result.Failure<IFreezableProcess>("Should have a single process on the top level.")));
        }

        private const string TypeString = "Do";
        private const string SetString = "Set";
        private const string EqualToString = "To";

        private static Result<Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>>> FromSimpleObject(object simpleObject, ProcessFactoryStore processFactoryStore)
        {
            Result<Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>>> result;

            if (simpleObject is List<object> list)
            {
                result = list.Select(x => FromSimpleObject(x, processFactoryStore))
                    .Select(x => x.Bind(y => y.Join(Result.Success,
                        r => Result.Failure<IFreezableProcess>("Cannot have a list of list"))))
                    .Combine()
                    .Map(x => new Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>>(x.ToList()));
            }
            else if (simpleObject is Dictionary<object, object> dictionary1 && dictionary1.ContainsKey(TypeString))
            {
                result = dictionary1.TryFindOrFail(TypeString, $"Object did not have {TypeString} set.")
                    .BindCast<object, string>()
                    .Bind(x => processFactoryStore.Dictionary.TryFindOrFail(x,
                        $"Could not find the process: '{x}'."))
                    .Compose(() =>
                        dictionary1.Where(x => x.Key.ToString() != TypeString)
                            .Select(x =>
                                FromSimpleObject(x.Value, processFactoryStore)
                                    .Map(value => (x.Key.ToString(), value)))
                            .Combine())
                    .Bind(x => CreateProcess(x.Item1, x.Item2))
                    .Map(x => new Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>>(x));
            }
            else if (simpleObject is Dictionary<object, object> dictionary2 && dictionary2.ContainsKey(SetString))
            {
                result = dictionary2.TryFindOrFail(SetString, $"Set Value did not have {SetString} set.")
                    .BindCast<object, string>()
                    .Compose(()=> dictionary2.TryFindOrFail(EqualToString, $"Set Value did not have {EqualToString} set.")
                        .Bind(x=> FromSimpleObject(x, processFactoryStore))
                        .Bind(UnwrapEitherToProcess)
                    )
                    .Map(x=> new SetVariableFreezableProcess(x.Item1, x.Item2))


                    .Map(x => new Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>>(x));
            }
            else if (simpleObject is string sString1 && GetVariableRegex.TryMatch(sString1, out var variableMatch))
            {
                result = new Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>>(
                    new GetVariableFreezableProcess(variableMatch.Groups["variableName"].Value));
            }
            else if (simpleObject is string sString2 && SetVariableRegex.TryMatch(sString2, out var setVariableMatch))
                result = FromSimpleObject(setVariableMatch.Groups["value"].Value, processFactoryStore)
                    .Bind(UnwrapEitherToProcess)
                    .Map(x=> new Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>>(new SetVariableFreezableProcess(setVariableMatch.Groups["variableName"].Value, x)))
                    ;
            else if (simpleObject is string sString3)
            {
                if (EnumConstantRegex.TryMatch(sString3, out var m))
                {
                    result = processFactoryStore.EnumTypesDictionary
                        .TryFindOrFail(m.Groups["enumName"].Value,
                            $"Could not recognize enum '{m.Groups["enumName"].Value}'")
                        .Bind(x => Extensions.TryGetEnumValue(x, m.Groups["enumValue"].Value))
                        .Map(x => new Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>>(new ConstantFreezableProcess(x)));
                }

                else if (int.TryParse(sString3, out var i))
                {
                    result = Result.Success<Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>>>(
                    new ConstantFreezableProcess(i));
                }
                else
                {
                    result = Result.Success<Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>>>(
                    new ConstantFreezableProcess(sString3));
                }
            }
            else
                throw new ArgumentOutOfRangeException(nameof(simpleObject));

            return result;

            static Result<IFreezableProcess> CreateProcess(RunnableProcessFactory factory,
                            IEnumerable<(string key, Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>> value)> arguments)
            {
                var singleArguments = new Dictionary<string, IFreezableProcess>();
                var listArguments = new Dictionary<string, IReadOnlyList<IFreezableProcess>>();


                foreach (var (key, processList) in arguments)
                    processList.Switch(l => singleArguments.Add(key, l),
                        r => listArguments.Add(key, r));


                var process = new CompoundFreezableProcess(factory, singleArguments, listArguments);
                return process;
            }

            static Result<IFreezableProcess> UnwrapEitherToProcess(Either<IFreezableProcess, IReadOnlyList<IFreezableProcess>> either) =>
                either.IsLeft
                    ? Result.Success(either.Left)
                    : Result.Failure<IFreezableProcess>("Variable cannot be list");
        }

        private static readonly Regex GetVariableRegex = new Regex(@"\A<(?<variableName>[\w\d_]+)>\Z", RegexOptions.Compiled);
        private static readonly Regex SetVariableRegex = new Regex(@"\A<(?<variableName>[\w\d_]+?)>\s*=\s*(?<value>.+)\Z", RegexOptions.Compiled);

        private static readonly Regex EnumConstantRegex = new Regex(@"(?<enumName>[\w\d_]+)\.(?<enumValue>[\w\d_]+)");

        private static object ToSimpleObject(IFreezableProcess process)
        {
            return process switch
            {
                CompoundFreezableProcess compoundFreezableProcess => ToExpando(compoundFreezableProcess),
                ConstantFreezableProcess constantFreezableProcess => SimplifyConstantFreezableProcess(constantFreezableProcess),
                GetVariableFreezableProcess getVariableFreezableProcess =>
                "<" + getVariableFreezableProcess.VariableName + ">",
                NameHelper.MissingProcess _ => throw new SerializationException(
                    "Cannot serialize Missing Process"),
                SetVariableFreezableProcess setVariableFreezableProcess => SimplifySetVariable(setVariableFreezableProcess)
                 ,
                _ => throw new ArgumentOutOfRangeException(nameof(process))
            };

            static object ToExpando(CompoundFreezableProcess compoundFreezableProcess)
            {
                IDictionary<string, object> expandoObject = new ExpandoObject();

                expandoObject[TypeString] = compoundFreezableProcess.ProcessFactory.TypeName;

                foreach (var (key, value) in compoundFreezableProcess.ProcessArguments)
                    expandoObject[key] = ToSimpleObject(value);

                foreach (var (key, value) in compoundFreezableProcess.ProcessListArguments)
                {
                    var list = value.Select(ToSimpleObject).ToList();

                    expandoObject[key] = list;
                }

                return expandoObject;
            }

            static string SimplifyConstantFreezableProcess(ConstantFreezableProcess constantFreezableProcess)
            {
                if (constantFreezableProcess.Value.GetType().IsEnum)
                    return constantFreezableProcess.Value.GetType().Name + "." + constantFreezableProcess.Value;
                return constantFreezableProcess.Value.ToString()??"";
            }

            static object SimplifySetVariable(SetVariableFreezableProcess setVariableFreezableProcess)
            {
                if (!(setVariableFreezableProcess.Value is CompoundFreezableProcess)) //Basic case
                    return "<" + setVariableFreezableProcess.VariableName + "> = " + ToSimpleObject(setVariableFreezableProcess.Value);


                IDictionary<string, object> expandoObject = new ExpandoObject();

                expandoObject[SetString] = setVariableFreezableProcess.VariableName;
                expandoObject[EqualToString] = ToSimpleObject(setVariableFreezableProcess.Value);


                return expandoObject;


            }
        }

    }
}
