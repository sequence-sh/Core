using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Writes a file to the local file system.
    /// </summary>
    public sealed class WriteFileStepFactory : SimpleStepFactory<WriteFile, Unit>
    {
        private WriteFileStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<WriteFile, Unit> Instance { get; } = new WriteFileStepFactory();
    }
}