using System;

namespace Reductech.EDR.Processes.Attributes
{
    /// <summary>
    /// Use this attribute to explain the meaning of the default value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DefaultValueExplanationAttribute : Attribute
    {
        /// <summary>
        /// Create a new DefaultValueExplanationAttribute.
        /// </summary>
        /// <param name="explanation"></param>
        public DefaultValueExplanationAttribute(string explanation)
        {
            Explanation = explanation;
        }

        /// <summary>
        /// What the default value means.
        /// </summary>
        public string Explanation { get; }
    }
}