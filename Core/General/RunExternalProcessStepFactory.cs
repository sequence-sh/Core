using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Runs an external executable program.
    /// </summary>
    public sealed class RunExternalProcessStepFactory : SimpleStepFactory<RunExternalProcess, Unit>
    {
        private RunExternalProcessStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<RunExternalProcess, Unit> Instance { get; } = new RunExternalProcessStepFactory();
    }
}