using System;
using System.Collections.Generic;
using OneOf;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Compares Property Keys Case Insensitive.
    /// </summary>
    public class PropertyKeyComparer : IEqualityComparer<OneOf<string, int>>
    {
        private PropertyKeyComparer() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static IEqualityComparer<OneOf<string, int>> Instance { get; } = new PropertyKeyComparer();

        /// <summary>
        /// Case insensitive comparison.
        /// </summary>
        public bool Equals(OneOf<string, int> x, OneOf<string, int> y)
        {
            if (x.IsT0 && y.IsT0)
                return x.AsT0.Equals(y.AsT0, StringComparison.OrdinalIgnoreCase);

            if (x.IsT1 && y.IsT1)
                return x.AsT1.Equals(y.AsT1);

            return false;

        }

        /// <summary>
        /// Gets the hashcode.
        /// </summary>
        public int GetHashCode(OneOf<string, int> obj) => obj.Match(StringComparer.OrdinalIgnoreCase.GetHashCode, x => x);
    }
}