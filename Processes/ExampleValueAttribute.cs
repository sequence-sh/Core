using System;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Use this attribute to give an example of a valid property value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExampleValueAttribute : Attribute
    {
        /// <summary>
        /// Create a new ExampleValueAttribute.
        /// </summary>
        /// <param name="exampleValue"></param>
        public ExampleValueAttribute(object exampleValue)
        {
            ExampleValue = exampleValue;
        }

        /// <summary>
        /// The example of a valid argument.
        /// </summary>
        public object ExampleValue { get; }
    }
}