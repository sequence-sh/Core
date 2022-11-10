namespace Sequence.Core.Enums;

/// <summary>
/// Determines what relationship between two operands causes the compare operator to return true.
/// </summary>
public enum CompareOperator
{
    /// <summary>
    /// Sentinel value.
    /// </summary>
    None,

    /// <summary>
    /// The operands are equal
    /// </summary>
    [Display(Name = "==")]
    Equals,

    /// <summary>
    /// The operands are not equal
    /// </summary>
    [Display(Name = "!=")]
    NotEquals,

    /// <summary>
    /// The left operand is less than the right operand
    /// </summary>
    [Display(Name = "<")]
    LessThan,

    /// <summary>
    /// The left operand is less than or equal to the right operand
    /// </summary>
    [Display(Name = "<=")]
    LessThanOrEqual,

    /// <summary>
    /// The left operand is greater than the right operand
    /// </summary>
    [Display(Name = ">")]
    GreaterThan,

    /// <summary>
    /// The left operand is greater than or equal to the right operand
    /// </summary>
    [Display(Name = ">=")]
    GreaterThanOrEqual
}
