using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Unzip a file in the file system.
    /// </summary>
    public class UnzipStepFactory : SimpleStepFactory<Unzip, Unit>
    {
        private UnzipStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<Unzip, Unit> Instance { get; } = new UnzipStepFactory();
    }
}