using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// An instance of a generic type.
    /// </summary>
    public sealed class GenericTypeReference : ITypeReference, IEquatable<ITypeReference>
    {
        /// <summary>
        /// Create a new GenericTypeReference.
        /// </summary>
        public GenericTypeReference(Type genericType, IReadOnlyList<ITypeReference> childTypes)
        {
            GenericType = genericType;
            ChildTypes = childTypes;
        }

        /// <inheritdoc />
        public IEnumerable<VariableTypeReference> VariableTypeReferences => ImmutableList<VariableTypeReference>.Empty;

        /// <inheritdoc />
        public IEnumerable<ActualTypeReference> ActualTypeReferences => ImmutableList<ActualTypeReference>.Empty;

        /// <inheritdoc />
        public IEnumerable<ITypeReference> TypeArgumentReferences => ChildTypes;

        /// <summary>
        /// The generic type.
        /// </summary>
        public Type GenericType { get; }

        /// <summary>
        /// The generic type members.
        /// </summary>
        public IReadOnlyList<ITypeReference> ChildTypes { get; }


        /// <inheritdoc />
        public bool Equals(ITypeReference? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other is GenericTypeReference gtr && GenericType == gtr.GenericType &&
                   ChildTypes.SequenceEqual(gtr.ChildTypes);
        }
    }
}