using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable.injection
{
    /// <summary>
    /// Injects a value from the enumerator into a property of a loop's process.
    /// </summary>
    public class Injection
    {
        /// <summary>
        /// The property of the subProcess to inject.
        /// </summary>
        [Required]
        
        [YamlMember(Order = 2)]
        [ExampleValue("SearchTerm")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Property { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// The regex to use to extract the useful part of the element.
        /// The first match of the regex will be used.
        /// </summary>
        
        [YamlMember(Order = 3)]
        [DefaultValueExplanation("The entire value will be injected.")]
        [ExampleValue(@"\w+")]
        public string? Regex { get; set; }

        /// <summary>
        /// The template to apply to the element before injection.
        /// The string '$s' in the template will be replaced with the element.
        /// The template will be applied after the Regex.
        /// </summary>
        
        [YamlMember(Order = 4)]
        [ExampleValue("$s.txt")]
        [DefaultValueExplanation("The value will be injected on its own.")]
        public string? Template { get; set; }

        internal Result<string> GetPropertyValue(string s)
        {
            if (Regex != null)
            {
                var match = System.Text.RegularExpressions.Regex.Match(s, Regex);
                if (match.Success) s = match.Value;
                else return Result.Failure<string>($"Regex did not match '{s}'");
            }

            if (Template != null) s = Template.Replace("$s", s);

            return Result.Success(s);
        }
    }
}