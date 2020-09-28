using System;

namespace Reductech.EDR.Processes.Internal.Documentation
{
    /// <summary>
    /// A category of documentation.
    /// </summary>
    public class DocumentationCategory
    {
        /// <summary>
        /// Creates a new DocumentationCategory
        /// </summary>
        public DocumentationCategory(string header, Type? anchor = null)
        {
            Header = header;
            Anchor = anchor;
        }

        /// <summary>
        /// The header that will appear at the top of the category.
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// The anchor that will appear at the top of the category.
        /// </summary>
        public Type? Anchor { get; }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is DocumentationCategory dc && Header == dc.Header && Anchor == dc.Anchor;

        /// <inheritdoc />
        public override int GetHashCode() => Header.GetHashCode() + Anchor?.GetHashCode() ?? 0;
    }
}