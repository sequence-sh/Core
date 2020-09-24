using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// Builds process names.
    /// </summary>
    public interface IProcessNameBuilder
    {
        /// <summary>
        /// Gets the name of the process from the process arguments
        /// </summary>
        string GetFromArguments(FreezableProcessData freezableProcessData);
    }

    /// <summary>
    /// The default process name builder
    /// </summary>
    public class DefaultProcessNameBuilder : IProcessNameBuilder
    {
        /// <summary>
        /// The process type name.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Creates a new DefaultProcessNameBuilder.
        /// </summary>
        public DefaultProcessNameBuilder(string typeName) => TypeName = typeName;

        /// <inheritdoc />
        public string GetFromArguments(FreezableProcessData freezableProcessData)
        {
            var args = string.Join(", ", freezableProcessData
                .Dictionary
                .OrderBy(x=>x.Key)
                .Select(x => $"{x.Key}: {x.Value.MemberString}"));

            return $"{TypeName}({args})";
        }
    }

    /// <summary>
    /// Builds the name for a particular instance of a process.
    /// </summary>
    public class ProcessNameBuilderFromTemplate : IProcessNameBuilder
    {
        /// <summary>
        /// Create a new process name.
        /// </summary>
        /// <param name="templateString"></param>
        public ProcessNameBuilderFromTemplate(string templateString) => TemplateString = templateString;


        /// <summary>
        /// The template to use for this process. Use square brackets for parameters and angle brackets for list parameters.
        /// </summary>
        public string TemplateString { get; }

        /// <summary>
        /// Optional delimiter to use for list properties.
        /// </summary>
        public string ListDelimiter { get; set; } = "; ";

        /// <summary>
        /// Gets the name of the process from the process arguments
        /// </summary>
        public string GetFromArguments(FreezableProcessData freezableProcessData)
        {
            var replacedString = NameVariableRegex.Replace(TemplateString, GetReplacement);

            return replacedString;

            string GetReplacement(Match m)
            {
                var variableName = m.Groups["ArgumentName"].Value;

                var p = freezableProcessData.Dictionary.TryFindOrFail(variableName, null)
                    .Map(x=>x.Join(vn=>vn.ToString(),
                        fp=>fp.ProcessName,
                        l=> string.Join(ListDelimiter, l.Select(i=>i.ProcessName))))
                    .OnFailureCompensate(x=>Result.Success("Unknown"));

                return p.Value;
            }

        }

        private static readonly Regex NameVariableRegex = new Regex(@"\[(?<ArgumentName>[\w_][\w\d_]*)\]", RegexOptions.Compiled);
    }

}