using System;

namespace Reductech.EDR.Core.Attributes
{
    /// <summary>
    /// Allows you to define an alternative name for a property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class AliasAttribute : Attribute
    {
        /// <summary>
        /// Create a new AliasAttribute
        /// </summary>
        public AliasAttribute(string name) => Name = name;

        /// <summary>
        /// The alternative name for the property
        /// </summary>
        public string Name { get; }
    }
}
