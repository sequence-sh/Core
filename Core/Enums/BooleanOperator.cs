using System.ComponentModel.DataAnnotations;

namespace Reductech.EDR.Core.Enums
{

/// <summary>
/// A boolean operator.
/// </summary>
public enum BooleanOperator
{
    /// <summary>
    /// Sentinel value.
    /// </summary>
    None,

    /// <summary>
    /// Returns true if both left and right are true.
    /// </summary>
    [Display(Name = "&&")]
    And,

    /// <summary>
    /// Returns true if either left or right is true.
    /// </summary>
    [Display(Name = "||")]
    Or
}

}
