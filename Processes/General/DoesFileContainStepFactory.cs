using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Returns whether a file on the file system contains a particular string.
    /// </summary>
    public class DoesFileContainStepFactory : SimpleStepFactory<DoesFileContain, bool>
    {
        private DoesFileContainStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<DoesFileContain, bool> Instance { get; } = new DoesFileContainStepFactory();
    }
}