using System.Collections.Generic;

namespace Reductech.EDR.Processes.Internal.Documentation
{
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
