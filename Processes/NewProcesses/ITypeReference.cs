using System.Collections.Generic;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// Either a type itself, or the name of a variable with the same type.
    /// </summary>
    public interface ITypeReference
    {
        /// <summary>
        /// Gets the variable type references.
        /// </summary>
        IEnumerable<VariableTypeReference> VariableTypeReferences { get; }

        /// <summary>
        /// Gets the actual type references.
        /// </summary>
        IEnumerable<ActualTypeReference> ActualTypeReferences { get; }

        IEnumerable<ITypeReference> TypeArgumentReferences { get; }

    }
}