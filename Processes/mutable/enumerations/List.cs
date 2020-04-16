using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable.enumerations
{
    /// <summary>
    /// Enumerates through elements of a list.
    /// </summary>
    public class List : Enumeration
    {
        /// <inheritdoc />
        internal override Result<IEnumerationElements, ErrorList> TryGetElements(IProcessSettings processSettings)
        {
            var elements =
                new EagerEnumerationElements(Members.Select(m => new ProcessInjector(Inject.Select(i => (m, i))))
                    .ToList());
            return Result.Success<IEnumerationElements,ErrorList>(elements);
        }

        internal override string Name => $"[{string.Join(", ", Members)}]";
        internal override IEnumerable<string> GetArgumentErrors()
        {
            if (Members == null)
                yield return $"{nameof(Members)} is null";
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