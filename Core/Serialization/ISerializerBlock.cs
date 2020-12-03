using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Contributes to the serialized string
    /// </summary>
    public interface ISerializerBlock
    {

        /// <summary>
        /// Gets the segment of serialized text if possible
        /// </summary>
        public Task<Result<string>> TryGetSegmentTextAsync(IReadOnlyDictionary<string, StepProperty> dictionary,
            CancellationToken cancellationToken);

    }
}