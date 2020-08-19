using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// A custom process serializer.
    /// </summary>
    public interface ICustomSerializer
    {
        /// <summary>
        /// Serialize this data as a process of this type.
        /// </summary>
        Result<string> TrySerialize(FreezableProcessData data);

        /// <summary>
        /// Try to deserialize this data.
        /// </summary>
        Result<IFreezableProcess> TryDeserialize(string s, ProcessFactoryStore processFactoryStore, RunnableProcessFactory factory);
    }

    /// <summary>
    /// A custom process serializer.
    /// </summary>
    public class CustomSerializer : ICustomSerializer
    {
        /// <summary>
        /// Create a new CustomSerializer
        /// </summary>
        public CustomSerializer(string templateString, Regex matchRegex, params IDeserializerMapping[] mappings)
        {
            TemplateString = templateString;
            MatchRegex = matchRegex;
            Mappings = mappings;
        }

        /// <summary>
        /// The template string to use.
        /// </summary>
        public string TemplateString { get; }

        /// <summary>
        /// The mappings to use.
        /// </summary>
        public IReadOnlyCollection<IDeserializerMapping>  Mappings { get; }

        /// <summary>
        /// A regex which matches the serialized form of this process.
        /// </summary>
        public Regex MatchRegex { get; }

        /// <summary>
        /// The delimiter to use for lists.
        /// </summary>
        public string ListDelimiter { get; } = "; ";


        /// <inheritdoc />
        public Result<string> TrySerialize(FreezableProcessData data)
        {
            var errors = new List<string>();
            var replacedString = NameVariableRegex.Replace(TemplateString, GetReplacement);


            if (errors.Any())
                return Result.Failure<string>(string.Join(", ", errors));

            return replacedString;

            string GetReplacement(Match m)
            {
                var variableName = m.Groups["ArgumentName"].Value;

                var p = data.Dictionary.TryFindOrFail(variableName, null)
                    .Bind(x => x.Join<Result<string>>(vn => vn.Name,
                        fp => fp is ConstantFreezableProcess cp? cp.SerializeToYaml() : Result.Failure<string>("Cannot handle compound argument"),
                        l => Result.Failure<string>("Cannot handle list argument")));

                if(p.IsSuccess)
                    return p.Value;

                errors.Add(p.Error);
                return "Unknown";
            }
        }

        private static readonly Regex NameVariableRegex = new Regex(@"\[(?<ArgumentName>[\w_][\w\d_]*)\]", RegexOptions.Compiled);

        /// <inheritdoc />
        public Result<IFreezableProcess> TryDeserialize(string s, ProcessFactoryStore processFactoryStore, RunnableProcessFactory factory)
        {
            if (string.IsNullOrWhiteSpace(s))
                return Result.Failure<IFreezableProcess>("String was empty");


            if(!MatchRegex.TryMatch(s, out var match) || match.Length < s.Length)
                return Result.Failure<IFreezableProcess>("Regex did not match");

            var dict = new Dictionary<string, ProcessMember>();

            foreach (var mapping in Mappings)
            {
                if (!match.Groups.TryGetValue(mapping.GroupName, out var group))
                    return Result.Failure<IFreezableProcess>($"Regex group {mapping.GroupName} was not matched.");

                var mr = mapping.TryDeserialize(group.Value, processFactoryStore);

                if (mr.IsFailure) return mr.ConvertFailure<IFreezableProcess>();

                dict[mapping.PropertyName] = mr.Value;
            }

            var fpd = new FreezableProcessData(dict);

            return new CompoundFreezableProcess(factory, fpd);
        }
    }

    /// <summary>
    /// Maps a regex group to a property
    /// </summary>
    public interface IDeserializerMapping
    {
        /// <summary>
        /// The name of the regex group to match.
        /// </summary>
        string GroupName { get; }

        /// <summary>
        /// The name of the property to map to.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Try to turn the text of the regex group into a process member.
        /// </summary>
        Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore);
    }

    /// <summary>
    /// Deserializes a regex group into a Variable Name.
    /// </summary>
    public class VariableNameDeserializerMapping : IDeserializerMapping
    {
        public VariableNameDeserializerMapping(string groupName, string propertyName)
        {
            GroupName = groupName;
            PropertyName = propertyName;
        }

        /// <inheritdoc />
        public string GroupName { get; }

        /// <inheritdoc />
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore) => new ProcessMember(new VariableName(groupText));
    }

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

    /// <summary>
    /// Deserializes a regex group into a string
    /// </summary>
    public class StringDeserializerMapping : IDeserializerMapping
    {
        public StringDeserializerMapping(string groupName, string propertyName)
        {
            GroupName = groupName;
            PropertyName = propertyName;
        }

        /// <inheritdoc />
        public string GroupName { get; }

        /// <inheritdoc />
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore) => new ProcessMember(new ConstantFreezableProcess(groupText));
    }

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

    /// <summary>
    /// Deserializes a regex group into an enum.
    /// </summary>
    public class EnumDeserializerMapping<T> : IDeserializerMapping
    where T: Enum
    {
        public EnumDeserializerMapping(string groupName, string propertyName, Func<T, string> getName)
        {
            GroupName = groupName;
            PropertyName = propertyName;

            Dictionary = Extensions.GetEnumValues<T>().ToDictionary(getName, x => x);
        }

        /// <inheritdoc />
        public string GroupName { get; }

        /// <inheritdoc />
        public string PropertyName { get; }

        /// <summary>
        /// Enum mapping dictionary.
        /// </summary>
        public IReadOnlyDictionary<string, T> Dictionary { get; }

        /// <inheritdoc />
        public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore)
        {
            if (Dictionary.TryGetValue(groupText, out var t))
                return new ProcessMember(new ConstantFreezableProcess(t));
            return Result.Failure<ProcessMember>($"Could not parse '{t}' as a {typeof(T).Name}");
        }
    }



    /// <summary>
    /// A factory for creating runnable processes.
    /// </summary>
    public abstract class RunnableProcessFactory
    {
        /// <summary>
        /// Tries to get a reference to the output type of this process.
        /// </summary>
        public abstract Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData);

        /// <summary>
        /// Unique name for this type of process.
        /// </summary>
        public string TypeName => FormatTypeName(ProcessType);

        /// <summary>
        /// The type of this process.
        /// </summary>
        public abstract Type ProcessType { get; }


        /// <inheritdoc />
        public override string ToString() => TypeName;

        /// <summary>
        /// Builds the name for a particular instance of a process.
        /// </summary>
        public abstract IProcessNameBuilder ProcessNameBuilder { get; }

        /// <summary>
        /// Gets all enum types used by this RunnableProcess.
        /// </summary>
        public abstract IEnumerable<Type> EnumTypes { get; }

        /// <summary>
        /// If this variable is being set. Get the type reference it is being set to.
        /// </summary>
        public virtual Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableProcessData freezableProcessData) =>
            Maybe<ITypeReference>.None;

        /// <summary>
        /// Custom serializer to use for yaml serialization and deserialization.
        /// </summary>
        public virtual ICustomSerializer? CustomSerializer { get; } = null;


        /// <summary>
        /// Special requirements for this process.
        /// </summary>
        public virtual IEnumerable<Requirement> Requirements => ImmutableArray<Requirement>.Empty;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        protected abstract Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, FreezableProcessData freezableProcessData);

        /// <summary>
        /// Gets the type of this member.
        /// </summary>
        public MemberType GetExpectedMemberType(string name)
        {
            var propertyInfo = ProcessType.GetProperty(name);

            if (propertyInfo == null) return MemberType.NotAMember;

            if (propertyInfo.GetCustomAttribute<VariableNameAttribute>() != null) return MemberType.VariableName;
            if (propertyInfo.GetCustomAttribute<RunnableProcessPropertyAttribute>() != null) return MemberType.Process;
            if (propertyInfo.GetCustomAttribute<RunnableProcessListPropertyAttribute>() != null) return MemberType.ProcessList;

            return MemberType.NotAMember;

        }


        /// <summary>
        /// Try to create the instance of this type and set all arguments.
        /// </summary>
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext, FreezableProcessData freezableProcessData)
        {
            var instanceResult = TryCreateInstance(processContext, freezableProcessData);

            if (instanceResult.IsFailure) return instanceResult;

            var runnableProcess = instanceResult.Value;

            var errors = new List<string>();

            var instanceType = instanceResult.Value.GetType();

            var remainingVariableNameProperties = instanceType
                .GetProperties()
                .Where(x => x.PropertyType == typeof(VariableName))
                .Where(x => x.GetCustomAttribute<VariableNameAttribute>() != null)
                .ToDictionary(x=>x.Name);


            var remainingProperties = instanceType.GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessPropertyAttribute>() != null)
                .ToDictionary(x => x.Name);

            var remainingListProperties = instanceType.GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessListPropertyAttribute>() != null)
                .ToDictionary(x => x.Name);


            foreach (var (propertyName, processMember) in freezableProcessData.Dictionary)
            {
                Result SetVariableName(VariableName variableName)
                {
                    if (remainingVariableNameProperties.Remove(propertyName, out var pi))
                        pi.SetValue(instanceResult.Value, variableName);
                    else
                        return Result.Failure($"The property '{propertyName}' does not exist on type '{TypeName}'.");
                    return Result.Success();
                }

                Result SetArgument(IFreezableProcess freezableProcess)
                {
                    if (remainingProperties.Remove(propertyName, out var pi))
                    {
                        var argumentFreezeResult = freezableProcess.TryFreeze(processContext);
                        if (argumentFreezeResult.IsFailure)
                            errors.Add(argumentFreezeResult.Error);
                        else
                        {
                            if (pi.PropertyType.IsInstanceOfType(argumentFreezeResult.Value))
                                pi.SetValue(runnableProcess, argumentFreezeResult.Value); //This could throw an exception but we don't expect it.
                            else
                                return Result.Failure($"'{pi.Name}' cannot take the value '{argumentFreezeResult.Value}'");
                        }
                    }
                    else
                        return Result.Failure($"The property '{propertyName}' does not exist on type '{TypeName}'.");

                    return Result.Success();
                }

                Result SetArgumentList(IReadOnlyList<IFreezableProcess> processList)
                {
                    if (remainingListProperties.Remove(propertyName, out var listInfo))
                    {
                        var freezeResult = processList.Select(x => x.TryFreeze(processContext)).Combine()
                            .Map(x => x.ToImmutableArray());
                        if (freezeResult.IsFailure)
                            return freezeResult;

                        var genericType = listInfo.PropertyType.GenericTypeArguments.Single();
                        var listType = typeof(List<>).MakeGenericType(genericType);

                        var list = Activator.CreateInstance(listType);

                        foreach (var process in freezeResult.Value)
                            if (genericType.IsInstanceOfType(process))
                            {
                                var addMethod = listType.GetMethod(nameof(List<object>.Add))!;
                                addMethod.Invoke(list, new object?[] {process});
                            }
                            else
                                return Result.Failure($"'{process.Name}' does not have the type '{genericType.Name}'");


                        listInfo.SetValue(runnableProcess, list);

                        return Result.Success();
                    }
                    else
                        return Result.Failure($"The property '{propertyName}' does not exist on type '{TypeName}'.");
                }

                var r = processMember.Join(SetVariableName, SetArgument, SetArgumentList);
                if(r.IsFailure) errors.Add(r.Error);
            }


            errors.AddRange(remainingVariableNameProperties.Values
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null)
                .Select(property => $"The property '{property.Name}' was not set on type '{GetType().Name}'."));

            errors.AddRange(remainingProperties.Values
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null)
                .Select(property => $"The property '{property.Name}' was not set on type '{GetType().Name}'."));

            errors.AddRange(remainingListProperties.Values
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null)
                .Select(property => $"The property '{property.Name}' was not set on type '{GetType().Name}'."));


            if (errors.Any())
                return Result.Failure<IRunnableProcess>(string.Join("\r\n", errors));

            return Result.Success(runnableProcess);

        }


        /// <summary>
        /// Creates a typed generic IRunnableProcess with one type argument.
        /// </summary>
        protected static Result<IRunnableProcess> TryCreateGeneric(Type openGenericType, Type parameterType)
        {
            var genericType = openGenericType.MakeGenericType(parameterType);

            var r = Activator.CreateInstance(genericType);

            if (r is IRunnableProcess rp)
                return Result.Success(rp);

            return Result.Failure<IRunnableProcess>($"Could not create an instance of {openGenericType.Name}<{parameterType.Name}>");
        }

        /// <summary>
        /// Gets the name of the type, removing the backtick if it is a generic type.
        /// </summary>
        protected static string FormatTypeName(Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                var iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0) friendlyName = friendlyName.Remove(iBacktick);
            }

            return friendlyName;
        }
    }
}