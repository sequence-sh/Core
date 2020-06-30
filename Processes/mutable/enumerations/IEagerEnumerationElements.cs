using System.Collections.Generic;
using Reductech.EDR.Processes.Mutable.Injections;

namespace Reductech.EDR.Processes.Mutable.Enumerations
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