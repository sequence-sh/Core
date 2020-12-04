using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
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
        /// Tries to get actual type references
        /// </summary>
        Result<ActualTypeReference, IErrorBuilder> TryGetActualTypeReference(TypeResolver typeResolver);

        /// <summary>
        /// Tries to get the generic member type
        /// </summary>
        Result<ActualTypeReference, IErrorBuilder> TryGetGenericTypeReference(TypeResolver typeResolver, int argumentNumber);


        /// <summary>
        /// Tries to get actual type references
        /// </summary>
        Result<Maybe<ActualTypeReference>, IErrorBuilder> GetActualTypeReferenceIfResolvable(TypeResolver typeResolver);
    }


}