using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Mutable.Injections;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable.Enumerations
{
    /// <summary>
    /// Enumerates through elements of a list.
    /// </summary>
    public class List : Enumeration
    {
        /// <inheritdoc />
        public override Result<IEnumerationElements> TryGetElements(IProcessSettings processSettings)
        {
            if(Members == null)
                return Result.Failure<IEnumerationElements>($"{nameof(Members)} is null");

            var elements =
                new EagerEnumerationElements(Members.Select(m => new ProcessInjector(Inject.Select(i => (m as object, i))))
                    .ToList());

            return elements;
        }

        /// <inheritdoc />
        public override string Name => Members != null && Members.Any()?  $"[{string.Join(", ", Members)}]" : "Empty List";

        /// <inheritdoc />
        public override EnumerationStyle GetEnumerationStyle()
        {
            return EnumerationStyle.Eager;
        }

        /// <summary>
        /// The elements to iterate over.
        /// </summary>
        [Required]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public List<string> Members { get; set; }

        /// <summary>
        /// Property injections to use.
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
        public List<Injection> Inject { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}