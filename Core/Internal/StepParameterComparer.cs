using System;
using System.Collections.Generic;

namespace Reductech.EDR.Core.Internal;

/// <summary>
/// Comparer for Step Parameters
/// </summary>
public class StepParameterComparer : IEqualityComparer<IStepParameter>
{
    private StepParameterComparer() { }

    /// <summary>
    /// The Instance
    /// </summary>
    public static IEqualityComparer<IStepParameter> Instance { get; } = new StepParameterComparer();

    /// <inheritdoc />
    public bool Equals(IStepParameter? x, IStepParameter? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null)
            return false;

        if (y is null)
            return false;

        if (x.GetType() != y.GetType())
            return false;

        return x.Name == y.Name && x.StepType == y.StepType;
    }

    /// <inheritdoc />
    public int GetHashCode(IStepParameter obj)
    {
        return HashCode.Combine(obj.Name, obj.StepType);
    }
}
