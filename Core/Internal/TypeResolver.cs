using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Gets the actual type from a type reference.
    /// </summary>
    public sealed class TypeResolver
    {
        /// <summary>
        /// Create a new TypeResolver
        /// </summary>
        public TypeResolver(StepFactoryStore stepFactoryStore, Dictionary<VariableName, ActualTypeReference>? myDictionary = null)
        {
            StepFactoryStore = stepFactoryStore;
            MyDictionary = myDictionary?? new Dictionary<VariableName, ActualTypeReference>();
        }


        /// <summary>
        /// Copy this type resolver.
        /// </summary>
        public TypeResolver Copy()
        {
            var dictClone = MyDictionary.ToDictionary(x => x.Key, x => x.Value);
            return new TypeResolver(StepFactoryStore, dictClone);
        }

        /// <inheritdoc />
        public override string ToString() => Dictionary.Count + " Types";

        private Dictionary<VariableName, ActualTypeReference> MyDictionary { get; }

        /// <summary>
        /// The dictionary mapping VariableNames to ActualTypeReferences
        /// </summary>
        public IReadOnlyDictionary<VariableName, ActualTypeReference> Dictionary => MyDictionary;

        /// <summary>
        /// The StepFactoryStory
        /// </summary>
        public StepFactoryStore StepFactoryStore { get; }



        /// <summary>
        /// Tries to add a variableName with this type to the type resolver.
        /// </summary>
        public Result<bool, IErrorBuilder> TryAddType(VariableName variable, ITypeReference typeReference)
        {
            var actualType = typeReference. GetActualTypeReferenceIfResolvable(this);
            if (actualType.IsFailure) return actualType.ConvertFailure<bool>();

            if (actualType.Value.HasNoValue) return false;

            var actualTypeReference = actualType.Value.Value;

            if (MyDictionary.TryGetValue(variable, out var previous))
            {
                if (previous.Equals(actualTypeReference))
                    return true;

                return new ErrorBuilder($"The type of {variable} is ambiguous between {actualTypeReference} and {previous}.", ErrorCode.AmbiguousType);
            }

            MyDictionary.Add(variable, actualTypeReference);
            return true;
        }
    }
}