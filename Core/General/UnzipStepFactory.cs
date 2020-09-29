using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
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