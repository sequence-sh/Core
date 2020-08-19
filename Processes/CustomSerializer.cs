using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
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
                        fp => fp is ConstantFreezableProcess cp? cp.SerializeToYaml().Trim() : Result.Failure<string>("Cannot handle compound argument"),
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
}