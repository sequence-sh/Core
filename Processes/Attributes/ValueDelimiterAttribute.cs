using System;

namespace Reductech.EDR.Processes.Attributes
{
    /// <summary>
    /// Indicates the value to be used as a delimiter
    /// </summary>
    public sealed class ValueDelimiterAttribute : Attribute {


        /// <summary>
        /// Create a new ValueDelimiterAttribute
        /// </summary>
        /// <param name="delimiter"></param>
        public ValueDelimiterAttribute(string delimiter)
        {
            Delimiter = delimiter;
        }

        /// <summary>
        /// The delimiter.
        /// </summary>
        public string Delimiter { get;  }
    }
}