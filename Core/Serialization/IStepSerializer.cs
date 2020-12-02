using System.Collections.Generic;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// A custom step serializer.
    /// </summary>
    public interface IStepSerializer
    {
        /// <summary>
        /// Serialize a step according to it's properties.
        /// </summary>
        string Serialize(IEnumerable<StepProperty> stepProperties);

    }
}