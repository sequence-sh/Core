namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// The multiplicity of the property
    /// </summary>
    public enum Multiplicity
    {
        /// <summary>
        /// Any number of values - a list
        /// </summary>
        Any,
        /// <summary>
        /// At least one value - a non-empty list
        /// </summary>
        AtLeastOne,
        /// <summary>
        /// Exactly one value
        /// </summary>
        ExactlyOne,
        /// <summary>
        /// Either one or zero values
        /// </summary>
        UpToOne
    }
}