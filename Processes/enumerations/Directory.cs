using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Processes.enumerations
{
    /// <summary>
    /// Enumerates through files in a directory
    /// </summary>
    public class Directory : Enumeration
    {
        internal override string Name => $"'{Path}'";

        internal override IEnumerable<IReadOnlyCollection<(string element, Injection injection)>> Elements
        {
            get
            {
                if (!System.IO.Directory.Exists(Path))
                    return Enumerable.Empty<IReadOnlyCollection<(string element, Injection injection)>>();


                var files = System.IO.Directory.GetFiles(Path);
                return files.Select(f => Injections.Select(i => (f, i)).ToList());
            }
        }

        /// <summary>
        /// The path to the directory
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Path { get; set; }

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