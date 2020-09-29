using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
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