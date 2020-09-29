using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Creates a new directory in the file system.
    /// </summary>
    public class CreateDirectoryStepFactory : SimpleStepFactory<CreateDirectory, Unit>
    {
        private CreateDirectoryStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<CreateDirectory, Unit> Instance { get; } = new CreateDirectoryStepFactory();
    }
}