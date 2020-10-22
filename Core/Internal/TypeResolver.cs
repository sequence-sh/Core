using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Gets the actual type from a type reference.
    /// </summary>
    public sealed class TypeResolver
    {
        /// <inheritdoc />
        public override string ToString() => Dictionary.Count + " Types";

        private Dictionary<VariableName, ActualTypeReference> MyDictionary { get; } = new Dictionary<VariableName, ActualTypeReference>();

        /// <summary>
        /// The dictionary mapping VariableNames to ActualTypeReferences
        /// </summary>
        public IReadOnlyDictionary<VariableName, ActualTypeReference> Dictionary => MyDictionary;


        /// <summary>
        /// Tries to add another actual type.
        /// </summary>
        public Result<Unit, IErrorBuilder> TryAddType(VariableName variable, ActualTypeReference actualTypeReference)
        {
            if (MyDictionary.TryGetValue(variable, out var previous))
            {
                if(previous.Equals(actualTypeReference))
                    return Unit.Default;

                return new ErrorBuilder($"The type of {variable} is ambiguous.", ErrorCode.AmbiguousType);
            }

            MyDictionary.Add(variable, actualTypeReference);
            return Unit.Default;
        }
    }
}