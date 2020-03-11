using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.enumerations
{
    /// <summary>
    /// Injects a value from the enumerator into a process property in a foreach loop
    /// </summary>
    public class Injection
    {
        /// <summary>
        /// The property of the subProcess to injection with the element of enumeration
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 2)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Property { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// The regex to use to extract the useful part of the element.
        /// The first match of the regex will be used.
        /// Will be ignored if null.
        /// </summary>
        [DataMember]
        [YamlMember(Order = 3)]
        public string? Regex { get; set; }

        /// <summary>
        /// The template to apply to the element before injection.
        /// If null the element will be used without modification.
        /// The string '$s' in the template will be replaced with the element.
        /// Is applied after the Regex.
        /// </summary>
        [DataMember]
        [YamlMember(Order = 4)]
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