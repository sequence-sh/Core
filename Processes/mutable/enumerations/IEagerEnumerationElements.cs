using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.mutable.injection;

namespace Reductech.EDR.Utilities.Processes.mutable.enumerations
{
    /// <summary>
    /// Elements of an enumeration, evaluated eagerly.
    /// </summary>
    public interface IEagerEnumerationElements : IEnumerationElements
    {
        /// <summary>
        /// One process injector for each row in the enumeration.
        /// </summary>
        IReadOnlyCollection<IProcessInjector> Injectors { get; }
    }
}