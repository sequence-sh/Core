using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.mutable.injection;

namespace Reductech.EDR.Utilities.Processes.mutable.enumerations
{
    internal class EagerEnumerationElements : IEnumerationElements
    {
        public EagerEnumerationElements(IReadOnlyCollection<IProcessInjector> injectors)
        {
            Injectors = injectors;
        }

        public IReadOnlyCollection<IProcessInjector> Injectors { get; }
    }
}