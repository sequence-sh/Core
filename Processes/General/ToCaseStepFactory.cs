using System;
using System.Collections.Generic;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Converts a string to a particular case.
    /// </summary>
    public sealed class ToCaseStepFactory : SimpleStepFactory<ToCase, string>
    {
        private ToCaseStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ToCase, string> Instance { get; } = new ToCaseStepFactory();

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] {typeof(TextCase)};
    }
}