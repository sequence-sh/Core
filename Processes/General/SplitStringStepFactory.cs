using System.Collections.Generic;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Splits a string.
    /// </summary>
    public class SplitStringStepFactory : SimpleStepFactory<SplitString, List<string>>
    {
        private SplitStringStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<SplitString, List<string>> Instance { get; } = new SplitStringStepFactory();
    }
}