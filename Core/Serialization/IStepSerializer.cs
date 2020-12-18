using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Serialization
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