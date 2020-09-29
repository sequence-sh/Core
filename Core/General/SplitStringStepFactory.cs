using System.Collections.Generic;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
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