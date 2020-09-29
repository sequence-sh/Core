using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Builds the name for a particular instance of a step.
    /// </summary>
    public class StepNameBuilderFromTemplate : IStepNameBuilder
    {
        /// <summary>
        /// Create a new step name.
        /// </summary>
        /// <param name="templateString"></param>
        public StepNameBuilderFromTemplate(string templateString) => TemplateString = templateString;


        /// <summary>
        /// The template to use for this step. Use square brackets for parameters and angle brackets for list parameters.
        /// </summary>
        public string TemplateString { get; }

        /// <summary>
        /// Optional delimiter to use for list properties.
        /// </summary>
        public string ListDelimiter { get; set; } = "; ";

        /// <summary>
        /// Gets the name of the step from the step arguments
        /// </summary>
        public string GetFromArguments(FreezableStepData freezableStepData)
        {
            var replacedString = NameVariableRegex.Replace(TemplateString, GetReplacement);

            return replacedString;

            string GetReplacement(Match m)
            {
                var variableName = m.Groups["ArgumentName"].Value;

                var p = freezableStepData.Dictionary.TryFindOrFail(variableName, null)
                    .Map(x=>x.Join(vn=>vn.ToString(),
                        fp=>fp.StepName,
                        l=> string.Join(ListDelimiter, Enumerable.Select<IFreezableStep, string>(l, i=>i.StepName))))
                    .OnFailureCompensate(x=>Result.Success("Unknown"));

                return p.Value;
            }

        }

        private static readonly Regex NameVariableRegex = new Regex(@"\[(?<ArgumentName>[\w_][\w\d_]*)\]", RegexOptions.Compiled);
    }
}