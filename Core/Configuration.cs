using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reductech.EDR.Core
{

/// <summary>
/// Additional configuration that may be needed in some use cases.
/// </summary>
[Serializable]
public sealed class Configuration
{
    /// <summary>
    /// Additional requirements, beyond the default for this step.
    /// </summary>
    [JsonProperty]
    public List<Requirement>? AdditionalRequirements { get; set; }

    /// <summary>
    /// Tags that the target machine must have (defined in a the config file) for this to be run on that machine.
    /// </summary>
    [JsonProperty]
    public List<string>? TargetMachineTags { get; set; }

    /// <summary>
    /// Conditional true, this step will not be split into multiple steps.
    /// </summary>
    [JsonProperty]
    public bool DoNotSplit { get; set; }

    /// <summary>
    /// The priority of this step. InitialSteps with higher priorities will be run first.
    /// </summary>
    [JsonProperty]
    public byte? Priority { get; set; }

    /// <summary>
    /// Combines two step configurations, deferring to the child where there is a conflict.
    /// </summary>
    public static Configuration? Combine(Configuration? parent, Configuration? child)
    {
        if (parent == null)
            return child;

        if (child == null)
            return parent;

        return new Configuration
        {
            AdditionalRequirements =
                Combine(parent.AdditionalRequirements, child.AdditionalRequirements, true),
            TargetMachineTags =
                Combine(parent.TargetMachineTags, child.TargetMachineTags, true),
            DoNotSplit = parent.DoNotSplit || child.DoNotSplit,
            Priority   = child.Priority ?? parent.Priority
        };
    }

    private static List<T>? Combine<T>(List<T>? l1, List<T>? l2, bool distinct)
    {
        if (l1 == null)
            return l2;

        if (l2 == null)
            return l1;

        return distinct ? l1.Concat(l2).Distinct().ToList() : l1.Concat(l2).ToList();
    }
}

}
