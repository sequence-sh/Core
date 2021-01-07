using System;

namespace Reductech.EDR.Core.Attributes
{

/// <summary>
/// Indicates the allowed range of values.
/// </summary>
public sealed class AllowedRangeAttribute : Attribute
{
    /// <summary>
    /// Creates a new AllowedRangeAttribute.
    /// </summary>
    /// <param name="allowedRangeValue"></param>
    public AllowedRangeAttribute(string allowedRangeValue)
    {
        AllowedRangeValue = allowedRangeValue;
    }

    /// <summary>
    /// The range allowed.
    /// </summary>
    public string AllowedRangeValue { get; }
}

}
