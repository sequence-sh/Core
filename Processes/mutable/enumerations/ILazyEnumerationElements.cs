using System.Collections.Generic;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Mutable.Enumerations
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