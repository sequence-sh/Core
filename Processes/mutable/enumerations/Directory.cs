using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable.enumerations
{
    /// <summary>
    /// Enumerates through files in a directory.
    /// </summary>
    public class Directory : Enumeration
    {
        /// <inheritdoc />
        internal override Result<IEnumerationElements, ErrorList> TryGetElements(IProcessSettings processSettings)
        {
            if (!System.IO.Directory.Exists(Path))
                return Result.Failure<IEnumerationElements,ErrorList>(
                    new ErrorList(){$"Directory '{Path}' does not exist"});


            var files = System.IO.Directory.GetFiles(Path);
            var injectors = files.Select(f => new ProcessInjector(Injection.Select(i => (f, i)))).ToList();


            return Result.Success<IEnumerationElements,ErrorList>(new EagerEnumerationElements(injectors));
        }

        internal override string Name => $"'{Path}'";
        internal override IEnumerable<string> GetArgumentErrors()
        {
            if (!System.IO.Directory.Exists(Path))
                yield return $"Directory '{Path}' does not exist";
        }

        /// <summary>
        /// The path to the directory.
        /// </summary>
        [Required]
        
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Path { get; set; }

        /// <summary>
        /// Property injections to use.
        /// </summary>
        [Required]
        
        [YamlMember(Order = 2)]
        public List<Injection> Injection { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}