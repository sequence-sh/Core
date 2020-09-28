using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Returns an error if the nested step does not return true.
    /// </summary>
    public sealed class AssertTrueStepFactory : SimpleStepFactory<AssertTrue, Unit>
    {
        private AssertTrueStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<AssertTrue, Unit> Instance { get; } = new AssertTrueStepFactory();
    }
}