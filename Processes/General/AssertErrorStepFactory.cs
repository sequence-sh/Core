using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Returns success if the Test step returns an error and a failure otherwise.
    /// </summary>
    public sealed class AssertErrorStepFactory : SimpleStepFactory<AssertError, Unit>
    {
        private AssertErrorStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<AssertError, Unit> Instance { get; } = new AssertErrorStepFactory();
    }
}