using System;
using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Core.Internal
{

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
        if (step is not ICompoundStep cs)
            return ArraySegment<Requirement>.Empty;

        var baseRequirements    = cs.StepFactory.Requirements.ToList();
        var runtimeRequirements = cs.RuntimeRequirements.ToList();

        var rRequirements = baseRequirements.Concat(runtimeRequirements).ToHashSet();
        return rRequirements;
    }
}

}
