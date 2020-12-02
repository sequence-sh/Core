using System.Collections.Generic;
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
        public Result<string> TryGetSegmentText(IReadOnlyDictionary<string, StepProperty> dictionary) => " ";
    }
}