using System;
using System.Collections.Generic;

namespace Reductech.EDR.Core.Internal.Documentation
{

/// <summary>
/// The parameter to a runnable method.
/// </summary>
public interface IParameter
{
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Aliases for this parameter
    /// </summary>
    IReadOnlyCollection<string> Aliases { get; }

    /// <summary>
    /// /// A summary of what this parameter does.
    /// </summary>
    string Summary { get; }

    /// <summary>
    /// The type of the parameter.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Is this parameter required.
    /// </summary>
    bool Required { get; }

    /// <summary>
    /// The position, if this parameter can be used as a positional parameter
    /// </summary>
    int? Position { get; }

    /// <summary>
    /// Extra fields e.g. Examples, Default Values, Requirements
    /// </summary>
    IReadOnlyDictionary<string, string> ExtraFields { get; }
}

}
