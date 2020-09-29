using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Reads text from a file.
    /// </summary>
    public sealed class ReadFileStepFactory : SimpleStepFactory<ReadFile, string>
    {
        private ReadFileStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ReadFile, string> Instance { get; } = new ReadFileStepFactory();
    }
}