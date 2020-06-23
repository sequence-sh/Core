using System;

namespace Reductech.EDR.Utilities.Processes.attributes
{
    /// <summary>
    /// Indicates that this is parameter should not be used by YAML
    /// </summary>
    public sealed class NotAParameterAttribute : Attribute {

        /// <summary>
        /// Creates a new NotAParameterAttribute
        /// </summary>
        public NotAParameterAttribute()
        {
        }

    }
}