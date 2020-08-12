using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// Builds the name for a particular instance of a process.
    /// </summary>
    public class ProcessNameBuilder
    {
        /// <summary>
        /// Create a new process name.
        /// </summary>
        /// <param name="templateString"></param>
        public ProcessNameBuilder(string templateString) => TemplateString = templateString;


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
                    .Map(x=>x.Join(vn=>vn.Name,
                        fp=>fp.ProcessName,
                        l=> string.Join(ListDelimiter, l.Select(i=>i.ProcessName))))
                    .OnFailureCompensate(x=>Result.Success("Unknown"));

                return p.Value;
            }

        }

        private static readonly Regex NameVariableRegex = new Regex(@"\[(?<ArgumentName>[\w_][\w\d_]*)\]", RegexOptions.Compiled);
    }

}