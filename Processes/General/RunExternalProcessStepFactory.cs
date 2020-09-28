using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
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