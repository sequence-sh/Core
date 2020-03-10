using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.enumerations
{
    /// <summary>
    /// Enumerates through files in a directory
    /// </summary>
    public class Directory : Enumeration
    {
        internal override string Name => $"'{Path}'";
        internal override IEnumerable<string> GetArgumentErrors()
        {
            if (!System.IO.Directory.Exists(Path))
                yield return $"Directory '{Path}' does not exist";
        }

        internal override Result<IReadOnlyCollection<IProcessInjector>,ErrorList> Elements
        {
            get
            {
                if (!System.IO.Directory.Exists(Path))
                    return Result.Failure<IReadOnlyCollection<IProcessInjector>,ErrorList>(
                        new ErrorList(){$"Directory '{Path}' does not exist"});


                var files = System.IO.Directory.GetFiles(Path);
                return Result.Success<IReadOnlyCollection<IProcessInjector>,ErrorList>
                    ( files.Select(f => new ProcessInjector(Injections.Select(i => (f, i)))).ToList());
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