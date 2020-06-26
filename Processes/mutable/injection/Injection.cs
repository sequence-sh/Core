using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable.injection
{
    /// <summary>
    /// Injects values from a CSV column into a property of a loop'obj process.
    /// </summary>
    public sealed class ColumnInjection : Injection
    {
        /// <summary>
        /// The column in the CSV to get the values from.
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
        [ExampleValue("SearchTerm")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Column { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    }


    /// <summary>
    /// Injects a value from the enumerator into a property of a loop'obj process.
    /// </summary>
    public class  Injection
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
        /// Only works for string properties.
        /// The first match of the regex will be used.
        /// </summary>
        [YamlMember(Order = 3)]
        [DefaultValueExplanation("The entire value will be injected.")]
        [ExampleValue(@"\w+")]
        public string? Regex { get; set; }

        /// <summary>
        /// The template to apply to the element before injection.
        /// Only works for string properties.
        /// The string '$1' in the template will be replaced with the element.
        /// The template will be applied after the Regex.
        /// </summary>
        [YamlMember(Order = 4)]
        [ExampleValue("$1.txt")]
        [DefaultValueExplanation("The value will be injected on its own.")]
        public string? Template { get; set; }

        /// <summary>
        /// Gets the property value of the object.
        /// If Regex of Template is set, the object will be converted to a string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal Result<object> GetPropertyValue(object obj)
        {
            if (Regex == null && Template == null)
                return obj;

            var s = obj.ToString();

            if (Regex != null)
            {
                var match = System.Text.RegularExpressions.Regex.Match(s, Regex);
                if (match.Success) s = match.Value;
                else return Result.Failure<string>($"Regex did not match '{s}'");
            }

            if (Template != null) s = Template.Replace("$1", s);

            return Result.Success(s);
        }

        /// <summary>
        /// Injects the element into the process.
        /// </summary>
        internal Result TryInject(object element, Process process)
        {
            var pathResult = InjectionParser.TryParse(Property);

            if (pathResult.IsFailure)
                return pathResult;

            var propertyValueResult = GetPropertyValue(element);
            if (propertyValueResult.IsFailure)
                return propertyValueResult;

            var setResult = pathResult.Value.TrySetValue(process, propertyValueResult.Value);

            return setResult;
        }
    }
}