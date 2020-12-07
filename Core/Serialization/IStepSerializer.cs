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
        Task<string> SerializeAsync(IEnumerable<StepProperty> stepProperties, CancellationToken cancellationToken);

    }
}