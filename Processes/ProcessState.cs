using System.Collections.Generic;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Represents the current state of a running process
    /// </summary>
    public class ProcessState
    {
        /// <summary>
        /// Artifacts of this process
        /// </summary>
        public Dictionary<string, object> Artifacts = new Dictionary<string, object>();
    }
}