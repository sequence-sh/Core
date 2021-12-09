using System.Collections.Generic;
using System.Reflection;

namespace Reductech.EDR.Core.Internal;

/// <summary>
/// Contains methods to help with step requirements
/// </summary>
public static class RequirementsHelpers
{
    /// <summary>
    /// Gets all requirements for a step and nested steps
    /// </summary>
    public static IEnumerable<Requirement> GetAllRequirements(this IStep step)
    {
        var requirements = new HashSet<Requirement>();

        var stepAssemblyName = step.GetType().Assembly.GetName().Name;

        if (stepAssemblyName is not null && stepAssemblyName != CoreAssemblyName)
        {
            requirements.Add(new ConnectorRequirement(stepAssemblyName));
        }

        if (step is ICompoundStep cs)
        {
            requirements.UnionWith(cs.StepFactory.Requirements);
            requirements.UnionWith(cs.RuntimeRequirements);
        }

        return requirements;
    }

    private static readonly string CoreAssemblyName =
        Assembly.GetAssembly(typeof(IStep))!.GetName().Name!;
}
