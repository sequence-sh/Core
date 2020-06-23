using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.mutable.enumerations
{
    /// <summary>
    /// The elements of an enumeration, evaluated lazily.
    /// </summary>
    public interface ILazyEnumerationElements : IEnumerationElements
    {
        /// <summary>
        /// The name of these elements.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Enumerates this process.
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<IProcessOutput<IEagerEnumerationElements>> Execute();
    }
}