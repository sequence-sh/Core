using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Include a required space in serialization.
    /// </summary>
    public class SpaceComponent :ISerializerBlock
    {
        private SpaceComponent() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SpaceComponent Instance { get; } = new SpaceComponent();

        /// <inheritdoc />
        public async Task<Result<string>> TryGetSegmentTextAsync(IReadOnlyDictionary<string, StepProperty> dictionary,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return " ";
        }
    }
}