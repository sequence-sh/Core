namespace Reductech.EDR.Processes.Util
{
    /// <summary>
    /// The result of a step with not return value.
    /// </summary>
    public sealed class Unit
    {
        /// <summary>
        /// The Unit.
        /// </summary>
        public static readonly Unit Default = new Unit();

        private Unit() {}
    }
}