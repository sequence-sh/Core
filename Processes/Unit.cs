namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Represents the output of a void method.
    /// </summary>
    public class Unit
    {
        /// <summary>
        /// The instance of Unit.
        /// </summary>
        public static readonly Unit Instance = new Unit();

        /// <summary>
        /// Private constructor for singleton.
        /// </summary>
        private Unit()
        {
        }
    }
}
