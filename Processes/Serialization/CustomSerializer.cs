using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.General;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// A component of a custom serializer
    /// </summary>
    public interface ICustomSerializerComponent
    {
        /// <summary>
        /// Contributes to the serialized string
        /// </summary>
        public ISerializerBlock? SerializerBlock { get; }

        /// <summary>
        /// Contributes to the deserialization regex
        /// </summary>
        public IDeserializerBlock? DeserializerBlock { get; }

        public IDeserializerMapping? Mapping { get; }
    }

    /// <summary>
    /// Contributes to the serialized string
    /// </summary>
    public interface ISerializerBlock
    {
        /// <summary>
        /// Gets the segment of serialized text.
        /// </summary>
        public Result<string> TryGetText(FreezableProcessData data);
    }

    /// <summary>
    /// Contributes to the deserialization regex
    /// </summary>
    public interface IDeserializerBlock
    {
        /// <summary>
        /// Gets the regex text to match this component
        /// </summary>
        public string GetRegexText(int index);
    }


    /// <summary>
    /// A custom process serializer.
    /// </summary>
    public class CustomSerializer : ICustomSerializer
    {
        /// <summary>
        /// Create a new CustomSerializer
        /// </summary>
        public CustomSerializer(params ICustomSerializerComponent[] components)//string templateString, Regex matchRegex, params IDeserializerMapping[] mappings)
        {
            Components = components;
            MatchRegex = CreateRegex(Components);
        }

        /// <summary>
        /// The component to use.
        /// </summary>
        public IReadOnlyCollection<ICustomSerializerComponent> Components { get; }

        private static Regex CreateRegex(IEnumerable<ICustomSerializerComponent> components)
        {
            var sb = new StringBuilder();

            sb.Append(@"\A\s*");

            foreach (var (component, index) in components.WithIndex())
                if (component.DeserializerBlock != null)
                    sb.Append(component.DeserializerBlock.GetRegexText(index));

            sb.Append(@"\s*\Z");

            return new Regex(sb.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        }

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
            StringBuilder sb = new StringBuilder();

            foreach (var serializerBlock in Components.Select(x => x.SerializerBlock).WhereNotNull())
            {
                var r = serializerBlock.TryGetText(data);
                if (r.IsFailure) return r;
                sb.Append(r.Value);
            }


            if (sb.Length == 0)
                return Result.Failure<string>("Serialized string was empty");

            return sb.ToString();

            //var errors = new List<string>();
            //var replacedString = NameVariableRegex.Replace(TemplateString, GetReplacement);


            //if (errors.Any())
            //    return Result.Failure<string>(string.Join(", ", errors));

            //return replacedString;

            //string GetReplacement(Match m)
            //{
            //    var variableName = m.Groups["ArgumentName"].Value;

            //    var p = data.Dictionary.TryFindOrFail(variableName, null)
            //        .Bind(x => x.Join(vn => vn.Name,
            //            TrySerialize,
            //            l => Result.Failure<string>("Cannot handle list argument")));

            //    if(p.IsSuccess)
            //        return p.Value;

            //    errors.Add(p.Error);
            //    return "Unknown";
            //}
        }

        private static Result<string> TrySerialize(IFreezableProcess fp)
        {
            if (fp is ConstantFreezableProcess cp)
            {
                if (cp.Value is string)
                    return $"'{cp.Value}'";
                return cp.SerializeToYaml().Trim();
            }
            else if (fp is CompoundFreezableProcess compound &&
                     compound.ProcessFactory == GetVariableProcessFactory.Instance)
                return fp.SerializeToYaml().Trim();

            return Result.Failure<string>("Could not serialize");
        }

        //private static readonly Regex NameVariableRegex = new Regex(@"\[(?<ArgumentName>[\w_][\w\d_]*)\]", RegexOptions.Compiled);

        /// <inheritdoc />
        public Result<IFreezableProcess> TryDeserialize(string s, ProcessFactoryStore processFactoryStore, RunnableProcessFactory factory)
        {
            if (string.IsNullOrWhiteSpace(s))
                return Result.Failure<IFreezableProcess>("String was empty");


            if(!MatchRegex.TryMatch(s, out var match) || match.Length < s.Length)
                return Result.Failure<IFreezableProcess>("Regex did not match");

            var dict = new Dictionary<string, ProcessMember>();

            foreach (var (mapping, index) in Components.Select(x=>x.Mapping).WithIndex())
            {
                if (mapping != null)
                {
                    var groupName = mapping.GetGroupName(index);

                    if (!match.Groups.TryGetValue(groupName, out var group))
                        return Result.Failure<IFreezableProcess>($"Regex group {groupName} was not matched.");

                    var mr = mapping.TryDeserialize(group.Value, processFactoryStore);

                    if (mr.IsFailure) return mr.ConvertFailure<IFreezableProcess>();

                    dict[mapping.PropertyName] = mr.Value;
                }
            }

            var fpd = new FreezableProcessData(dict);

            return new CompoundFreezableProcess(factory, fpd);
        }
    }
}