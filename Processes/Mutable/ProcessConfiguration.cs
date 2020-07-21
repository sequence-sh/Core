using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Combines two process configurations, deferring to the child where there is a conflict.
        /// </summary>
        public static ProcessConfiguration? Combine(ProcessConfiguration? parent, ProcessConfiguration? child)
        {
            if (parent == null)
                return child;
            if (child == null)
                return parent;

            return new ProcessConfiguration
            {
                AdditionalRequirements = Combine(parent.AdditionalRequirements, child.AdditionalRequirements, true),
                TargetMachineTags = Combine(parent.TargetMachineTags, child.TargetMachineTags, true),
                DoNotSplit = parent.DoNotSplit || child.DoNotSplit,
                Priority = child.Priority?? parent.Priority
            };

        }


        private static List<string>? Combine(List<string>? l1, List<string>? l2, bool distinct)
        {
            if (l1 == null) return l2;
            if (l2 == null) return l1;

            return distinct ? l1.Concat(l2).Distinct().ToList() : l1.Concat(l2).ToList();
        }
    }
}