using System.Collections.Generic;

namespace Reductech.EDR.Core.Internal.Serialization
{
    /// <summary>
    /// A custom step serializer.
    /// </summary>
    public interface IStepSerializer
    {
        /// <summary>
        /// SerializeAsync a step according to it's properties.
        /// </summary>
        string Serialize(IEnumerable<StepProperty> stepProperties);

    }
}