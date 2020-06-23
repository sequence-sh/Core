using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.mutable.injection;

namespace Reductech.EDR.Utilities.Processes.mutable.enumerations
{
    /// <summary>
    /// Elements of an enumeration, evaluated eagerly.
    /// </summary>
    public sealed class EagerEnumerationElements : IEagerEnumerationElements
    {
        /// <summary>
        /// Creates new EagerEnumerationElements
        /// </summary>
        /// <param name="injectors"></param>
        public EagerEnumerationElements(IReadOnlyCollection<IProcessInjector> injectors)
        {
            Injectors = injectors;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IProcessInjector> Injectors { get; }
    }
}