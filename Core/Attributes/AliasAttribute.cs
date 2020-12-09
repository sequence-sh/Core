using System;

namespace Reductech.EDR.Core.Attributes
{
    /// <summary>
    /// Allows you to define an alternative name for a property or step
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class AliasAttribute : Attribute
    {
        /// <summary>
        /// Create a new AliasAttribute
        /// </summary>
        public AliasAttribute(string name) => Name = name;

        /// <summary>
        /// The alternative name for the property or step
        /// </summary>
        public string Name { get; }
    }
}
