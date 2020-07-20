using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// Additional process configuration that may be needed in some use cases.
    /// </summary>
    public sealed class ProcessConfiguration
    {
        /// <summary>
        /// Additional requirements, beyond the default for this process.
        /// </summary>
        [YamlMember(Order = 1)]
        public List<string>? AdditionalRequirements { get; set; }

        /// <summary>
        /// Tags that the target machine must have (defined in a the config file) for this to be run on that machine.
        /// </summary>
        [YamlMember(Order = 2)]
        public List<string>? TargetMachineTags { get; set; }

        /// <summary>
        /// If true, this process will not be split into multiple subProcesses.
        /// </summary>
        [YamlMember(Order = 3)]
        public bool DoNotSplit { get; set; }

        /// <summary>
        /// The priority of this process. Processes with higher priorities will be run first.
        /// </summary>
        [YamlMember(Order = 5)]
        public double? Priority { get; set; }
    }
}