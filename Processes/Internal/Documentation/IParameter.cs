using System;
using System.Collections.Generic;

namespace Reductech.EDR.Processes.Internal.Documentation
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
        /// Extra fields e.g. Examples, Default Values, Requirements
        /// </summary>
        IReadOnlyDictionary<string, string> ExtraFields { get; }

        ///// <summary>
        ///// An example of a valid value for this parameter.
        ///// </summary>
        //string? Example { get; }

        ///// <summary>
        ///// The default value of this parameter, as a human readable string, if applicable.
        ///// </summary>
        //string? DefaultValueString { get; }

        ///// <summary>
        ///// Requirements for using this parameter.
        ///// </summary>
        //string? Requirements { get; }
    }
}
