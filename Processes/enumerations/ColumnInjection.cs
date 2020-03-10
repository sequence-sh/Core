using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.enumerations
{
    /// <summary>
    /// Inject the values from a particular column
    /// </summary>
    public class ColumnInjection : Injection
    {
        /// <summary>
        /// The header in the CSV
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Header { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}