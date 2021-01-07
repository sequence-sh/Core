using System.ComponentModel.DataAnnotations;

namespace Reductech.EDR.Core.Enums
{

/// <summary>
/// An operator to use for comparison.
/// </summary>
public enum CompareOperator
{
    /// <summary>
    /// Sentinel value.
    /// </summary>
    None,
    #pragma warning disable 1591
    [Display(Name = "==")] Equals,

    [Display(Name = "!=")] NotEquals,
    [Display(Name = "<")] LessThan,
    [Display(Name = "<=")] LessThanOrEqual,
    [Display(Name = ">")] GreaterThan,
    [Display(Name = ">=")] GreaterThanOrEqual
    #pragma warning restore 1591
}

}
