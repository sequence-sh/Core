using System.ComponentModel.DataAnnotations;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// An operator that can be applied to two numbers.
    /// </summary>
    public enum MathOperator
    {
        /// <summary>
        /// Sentinel value
        /// </summary>
        None,

        /// <summary>
        /// Add the left and right operands.
        /// </summary>
        [Display(Name = "+")]
        Add,
        /// <summary>
        /// Subtract the right operand from the left.
        /// </summary>
        [Display(Name = "-")]
        Subtract,
        /// <summary>
        /// Multiply the left and right operands.
        /// </summary>
        [Display(Name = "*")]
        Multiply,
        /// <summary>
        /// Divide the left operand by the right.
        /// </summary>
        [Display(Name = "/")]
        Divide,
        /// <summary>
        /// Reduce the left operand modulo the right.
        /// </summary>
        [Display(Name = "%")]
        Modulo,
        /// <summary>
        /// Raise the left operand to the power of the right.
        /// </summary>
        [Display(Name = "^")]
        Power

    }
}