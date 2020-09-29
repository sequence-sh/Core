using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Trims a string.
    /// </summary>
    public sealed class TrimStepFactory : SimpleStepFactory<Trim, string>
    {
        private TrimStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<Trim, string> Instance { get; } = new TrimStepFactory();

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] { typeof(TrimSide) };
    }
}