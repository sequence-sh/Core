using System.Collections.Generic;
using Reductech.EDR.Core.Attributes;

namespace Reductech.EDR.Core.Internal.Documentation
{

/// <summary>
/// Something that will be shown in the documentation
/// </summary>
public interface IDocumentedStep
{
    /// <summary>
    /// What category this item will belong in.
    /// </summary>
    string DocumentationCategory { get; }

    /// <summary>
    /// The main name of the step.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The name of the file
    /// </summary>
    string FileName { get; }

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

    /// <summary>
    /// All Names including aliases
    /// </summary>
    IReadOnlyList<string> AllNames { get; }

    /// <summary>
    /// Examples of this step's usage
    /// </summary>
    IReadOnlyList<SCLExampleAttribute> Examples { get; }
}

}
