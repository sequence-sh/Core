using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets a substring from a string.
    /// </summary>
    public sealed class GetSubstringStepFactory : SimpleStepFactory<GetSubstring, string>
    {
        private GetSubstringStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<GetSubstring, string> Instance { get; } = new GetSubstringStepFactory();
    }
}