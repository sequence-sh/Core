using System.ComponentModel.DataAnnotations;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// An operator to use for comparison.
    /// </summary>
    public enum CompareOperator
    {
        [Display(Name = "=")]
#pragma warning disable 1591
        Equals,

        [Display(Name = "!=")]
        NotEquals,
        [Display(Name = "<")]
        LessThan,
        [Display(Name = "<=")]
        LessThanOrEqual,
        [Display(Name = ">")]
        GreaterThan,
        [Display(Name = ">=")]
        GreaterThanOrEqual
#pragma warning restore 1591
    }
}