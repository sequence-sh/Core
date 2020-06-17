using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable.chain
{
    /// <summary>
    /// A step in the immutableChain other than the first.
    /// </summary>
    public class ChainLink : Chain
    {
        /// <summary>
        /// The injection to inject the result of the previous method.
        /// </summary>
        [YamlMember(Order = 3)]
        [Required]
        public Injection Inject { get; set; }
    }
}