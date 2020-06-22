using System;

namespace Reductech.EDR.Utilities.Processes.attributes
{
    /// <summary>
    /// Indicates the recommended range for this parameter.
    /// </summary>
    public sealed class RecommendedRangeAttribute : Attribute
    {
        /// <summary>
        /// Creates a new RecommendedRangeAttribute.
        /// </summary>
        /// <param name="recommendedRange"></param>
        public RecommendedRangeAttribute(string recommendedRange)
        {
            RecommendedRange = recommendedRange;
        }

        /// <summary>
        /// The recommended range for this parameter.
        /// </summary>
        public string RecommendedRange { get; }
    }
}