using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.mutable.enumerations
{
    /// <summary>
    /// Represents a list of elements
    /// </summary>
    public abstract class Enumeration
    {
        /// <summary>
        /// Try to get the elements of this enumeration.
        /// They will either be EagerEnumerationElements or Lazy EnumerationElements
        /// </summary>
        /// <param name="processSettings"></param>
        /// <returns></returns>
        internal abstract Result<IEnumerationElements> TryGetElements(IProcessSettings processSettings);
        internal abstract string Name { get; }

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