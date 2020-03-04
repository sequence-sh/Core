﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Processes.enumerations
{
    /// <summary>
    /// Enumerates through elements of a list
    /// </summary>
    public class Collection : Enumeration
    {
        internal override IEnumerable<IReadOnlyCollection<(string element, Injection injection)>> Elements =>
            Members.Select(m => Injections.Select(i => (m, i)).ToList());
        internal override string Name => $"[{string.Join(", ", Members)}]";

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