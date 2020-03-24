using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.mutable.injection;

namespace Reductech.EDR.Utilities.Processes.mutable.enumerations
{
    /// <summary>
    /// Represents a list of elements
    /// </summary>
    public abstract class Enumeration
    {
        internal abstract Result<IReadOnlyCollection<IProcessInjector>,ErrorList>  Elements { get; }
        internal abstract string Name { get; }

        /// <summary>
        /// Get errors in the properties of this enumeration.
        /// </summary>
        internal abstract IEnumerable<string> GetArgumentErrors();

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Enumeration e && e.Name == Name && GetType() == e.GetType();
        }
        
        /// <inheritdoc />
        public override int GetHashCode() => Name.GetHashCode();
    }
}