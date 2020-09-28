using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Something that will be shown in the documentation
    /// </summary>
    public interface IDocumented
    {
        /// <summary>
        /// What category this item will belong in.
        /// </summary>
        DocumentationCategory DocumentationCategory { get; }


        /// <summary>
        /// The name of the method.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A summary of what the method does.
        /// </summary>
        string Summary { get; }

        /// <summary>
        /// Information about the return type
        /// </summary>
        string? TypeDetails { get; }

        /// <summary>
        /// Requirements for using this method.
        /// </summary>
        IEnumerable<string> Requirements { get; }

        /// <summary>
        /// The parameters to the method.
        /// </summary>
        IEnumerable<IParameter> Parameters { get; }
    }
}
