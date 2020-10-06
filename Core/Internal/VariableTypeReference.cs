using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Indicates that this type is the same as that of the variable with the given name.
    /// </summary>
    public sealed class VariableTypeReference : ITypeReference, IEquatable<ITypeReference>
    {
        /// <summary>
        /// Creates a new VariableTypeReference.
        /// </summary>
        /// <param name="variableName"></param>
        public VariableTypeReference(VariableName variableName) => VariableName = variableName;

        /// <summary>
        /// The name of a variable with the same type as this type.
        /// </summary>
        public VariableName VariableName { get; }

        /// <inheritdoc />
        public bool Equals(ITypeReference? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other switch
            {
                ActualTypeReference _ => false,
                MultipleTypeReference multipleTypeReference => multipleTypeReference.AllReferences.Count == 1 &&
                                                               multipleTypeReference.AllReferences.Contains(this),
                VariableTypeReference typeReference => VariableName == typeReference.VariableName,
                GenericTypeReference _ => false,
                _ => throw new ArgumentOutOfRangeException(nameof(other))
            };
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ITypeReference other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => VariableName.GetHashCode();

        /// <inheritdoc />
        IEnumerable<VariableTypeReference> ITypeReference.VariableTypeReferences => new[] {this};

        /// <inheritdoc />
        public Result<ActualTypeReference> TryGetActualTypeReference(TypeResolver tr)
        {
            var r = tr.Dictionary.TryFindOrFail(VariableName, null);
            return r;
        }

        /// <inheritdoc />
        public Result<ActualTypeReference> TryGetGenericTypeReference(TypeResolver typeResolver, int argumentNumber)
        {
            var r = typeResolver.Dictionary.TryFindOrFail(VariableName, null)
                .Bind(x => x.TryGetGenericTypeReference(typeResolver, argumentNumber));

            return r;
        }
    }
}