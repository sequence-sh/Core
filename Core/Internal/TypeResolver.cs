using System.Collections.Generic;
using CSharpFunctionalExtensions;

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
        public Result TryAddType(VariableName variable, ActualTypeReference actualTypeReference)
        {
            if (MyDictionary.TryGetValue(variable, out var previous))
            {
                if(previous.Equals(actualTypeReference))
                    return Result.Success();

                return Result.Failure($"The type of {variable} is ambiguous.");
            }

            MyDictionary.Add(variable, actualTypeReference);
            return Result.Success();
        }
    }
}