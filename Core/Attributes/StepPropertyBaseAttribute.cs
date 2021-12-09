using System;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Attributes;

/// <summary>
/// A property that will be used by the step.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public abstract class StepPropertyBaseAttribute : Attribute
{
    /// <summary>
    /// Create a new StepPropertyBaseAttribute with a defined order
    /// </summary>
    protected StepPropertyBaseAttribute(int? order) => Order = order;

    /// <summary>
    /// Create a new StepPropertyBaseAttribute with no defined order (this property will not be usable as a positional parameter)
    /// </summary>
    protected StepPropertyBaseAttribute() => Order = null;

    /// <summary>
    /// The order where this property should appear.
    /// </summary>
    public int? Order { get; }

    /// <summary>
    /// The member type of this step property
    /// </summary>
    public abstract MemberType MemberType { get; }
}
