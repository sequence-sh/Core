namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// The type of the property
    /// </summary>
    public enum SchemaPropertyType
    {
        /// <summary>
        /// A string.
        /// </summary>
        String,
        /// <summary>
        /// An integer.
        /// </summary>
        Integer,
        /// <summary>
        /// A double precision number.
        /// </summary>
        Double,
        /// <summary>
        /// An enumeration of some sort.
        /// The format string will contain the possible values.
        /// </summary>
        Enum,
        /// <summary>
        /// A boolean.
        /// </summary>
        Bool,
        /// <summary>
        /// A date.
        /// </summary>
        Date

    }
}