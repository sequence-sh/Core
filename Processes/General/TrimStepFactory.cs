using System;
using System.Collections.Generic;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
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