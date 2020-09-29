using System.Collections.Generic;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Extracts elements from a CSV file
    /// </summary>
    public sealed class ReadCsvStepFactory : SimpleStepFactory<ReadCsv, List<List<string>>>
    {
        private ReadCsvStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ReadCsv, List<List<string>>> Instance { get; } = new ReadCsvStepFactory();
    }
}