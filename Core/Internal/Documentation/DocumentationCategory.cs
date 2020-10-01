using System;

namespace Reductech.EDR.Core.Internal.Documentation
{
    /// <summary>
    /// A category of documentation.
    /// </summary>
    public class DocumentationCategory : IEquatable<DocumentationCategory>

    {
    /// <summary>
    /// Creates a new DocumentationCategory
    /// </summary>
    public DocumentationCategory(string header) => Header = header;

    /// <summary>
    /// The header that will appear at the top of the category.
    /// </summary>
    public string Header { get; }

    /// <inheritdoc />
    public bool Equals(DocumentationCategory? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Header == other.Header;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is DocumentationCategory dc)
            return Equals(dc);
        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Header);
    }
}