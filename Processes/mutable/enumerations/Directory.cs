using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Mutable.Injections;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable.Enumerations
{
    /// <summary>
    /// Enumerates through files in a directory.
    /// </summary>
    public class Directory : Enumeration
    {
        /// <inheritdoc />
        public override Result<IEnumerationElements> TryGetElements(IProcessSettings processSettings)
        {
            if (!System.IO.Directory.Exists(Path))
                return Result.Failure<IEnumerationElements>($"Directory '{Path}' does not exist");

            var files = System.IO.Directory.GetFiles(Path);
            var injectors = files.Select(f => new ProcessInjector(Injection.Select(i => (f as object, i)))).ToList();

            return Result.Success<IEnumerationElements>(new EagerEnumerationElements(injectors));
        }

        /// <inheritdoc />
        public override string Name => $"'{Path}'";

        /// <inheritdoc />
        public override EnumerationStyle EnumerationStyle => EnumerationStyle.Lazy;

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