using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Mutable.Enumerations
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
        public abstract Result<IEnumerationElements> TryGetElements(IProcessSettings processSettings);

        /// <summary>
        /// The name of this enumeration.
        /// </summary>
        public abstract string Name { get; }

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

        /// <summary>
        /// Whether this will enumerate lazily or eagerly.
        /// </summary>
        public abstract EnumerationStyle EnumerationStyle
        {
            get;
        }
    }

    /// <summary>
    /// Whether to enumerate lazily or eagerly.
    /// </summary>
    public enum EnumerationStyle
    {
        /// <summary>
        /// The processes are only frozen at the last possible moment.
        /// </summary>
        Lazy,
        /// <summary>
        /// All processes are frozen before enumeration begins.
        /// </summary>
        Eager
    }
}