using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Serialization
{

    /// <summary>
    /// Deserializes a regex group into a constant of any type.
    /// </summary>
    public class StepComponent :  ISerializerBlock
    {
        /// <summary>
        /// Creates a new StepComponent
        /// </summary>
        /// <param name="propertyName"></param>
        public StepComponent(string propertyName) => PropertyName = propertyName;

        /// <summary>
        /// The property name
        /// </summary>
        public string PropertyName { get; }

        /// <inheritdoc />
        public async Task<Result<string>> TryGetSegmentTextAsync(IReadOnlyDictionary<string, StepProperty> dictionary,
            CancellationToken cancellationToken)
        {
            return await dictionary[PropertyName].SerializeValueAsync(cancellationToken);
        }
    }
}