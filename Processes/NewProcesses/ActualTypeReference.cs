using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// An actual instance of the type.
    /// </summary>
    public sealed class ActualTypeReference : ITypeReference, IEquatable<ITypeReference>
    {
        /// <summary>
        /// Creates a new ActualTypeReference.
        /// </summary>
        public ActualTypeReference(Type type) => Type = type;

        /// <summary>
        /// The type to use.
        /// </summary>
        public Type Type { get; }

        /// <inheritdoc />
        public bool Equals(ITypeReference? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other switch
            {
                ActualTypeReference actualType => Type == actualType.Type,
                MultipleTypeReference multipleTypeReference => multipleTypeReference.AllReferences.Count == 0 &&
                                                               multipleTypeReference.AllReferences.Contains(this),
                VariableTypeReference _ => false,
                GenericTypeReference _ => false,
                _ => throw new ArgumentOutOfRangeException(nameof(other))
            };
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ITypeReference other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Type.GetHashCode();

        /// <inheritdoc />
        IEnumerable<VariableTypeReference> ITypeReference.VariableTypeReferences => ImmutableArray<VariableTypeReference>.Empty;

        /// <inheritdoc />
        IEnumerable<ActualTypeReference> ITypeReference.ActualTypeReferences => new[] {this};

        /// <inheritdoc />
        public IEnumerable<ITypeReference> TypeArgumentReferences => ImmutableList<ITypeReference>.Empty;
    }
}