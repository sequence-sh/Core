using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Serializes an array
    /// </summary>
    public sealed class ArraySerializer : IStepSerializer
    {
        private ArraySerializer() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static IStepSerializer Instance { get; } = new ArraySerializer();

        /// <inheritdoc />
        public async Task<string> SerializeAsync(IEnumerable<StepProperty> stepProperties, CancellationToken cancellationToken)
        {
            return await stepProperties.Single().SerializeValueAsync(cancellationToken); // :)
        }
    }
}