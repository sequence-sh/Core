namespace Reductech.EDR.Processes
{
    /// <summary>
    /// The result of a process with not return value.
    /// </summary>
    public sealed class Unit
    {
        /// <summary>
        /// The Unit.
        /// </summary>
        public static readonly Unit Default = new Unit();

        private Unit()
        {
        }
    }
}