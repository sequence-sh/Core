using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Processes.enumerations
{
    /// <summary>
    /// Enumerates through elements of a list
    /// </summary>
    public class Collection : Enumeration
    {
        internal override Result<IReadOnlyCollection<IProcessInjector>,ErrorList> Elements =>
            Result.Success<IReadOnlyCollection<IProcessInjector>,ErrorList>
                (Members.Select(m => new ProcessInjector(Injections.Select(i => (m, i)))).ToList());
        internal override string Name => $"[{string.Join(", ", Members)}]";
        internal override IEnumerable<string> GetArgumentErrors()
        {
            if (Members == null)
                yield return $"{nameof(Members)} is null";
        }

        /// <summary>
        /// The elements to iterate over
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public List<string> Members { get; set; }

        /// <summary>
        /// Injections to use on the elements of the list
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 2)]
        public List<Injection> Injections { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}