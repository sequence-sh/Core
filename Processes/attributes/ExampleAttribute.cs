﻿using System;

namespace Reductech.EDR.Utilities.Processes.attributes
{
    /// <summary>
    /// Indicates an example value for this parameter.
    /// </summary>
    public sealed class ExampleAttribute : Attribute
    {
        /// <summary>
        /// Creates a new ExampleAttribute.
        /// </summary>
        /// <param name="example"></param>
        public ExampleAttribute(string example)
        {
            Example = example;
        }

        /// <summary>
        /// The example value.
        /// </summary>
        public string Example { get; }
    }
}