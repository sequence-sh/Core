﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A type that is the same a multiple different references.
    /// </summary>
    public sealed class MultipleTypeReference : ITypeReference, IEquatable<ITypeReference>
    {
        /// <summary>
        /// Tries to create a new MultipleTypeReference.
        /// </summary>
        public static Result<ITypeReference> TryCreate(IEnumerable<ITypeReference> references, string parentProcess)
        {
            var set = references.ToImmutableHashSet();

            switch (set.Count)
            {
                case 0:
                    return new ActualTypeReference(typeof(object)); //TODO type reference any???
                    //return Result.Failure<ITypeReference>($"Could not infer type for {parentProcess} as it has no children.");
                case 1:
                    return Result.Success(set.Single());
                default:
                {
                    if (set.OfType<ActualTypeReference>().Count() > 1)
                        return Result.Failure<ITypeReference>(
                            $"Could not infer type for {parentProcess} as it's children have different types ({string.Join(", ", set.OfType<ActualTypeReference>().Select(x=>x.Type.Name))}).");
                    return new MultipleTypeReference(set);
                }
            }
        }

        /// <summary>
        /// Creates a new MultipleTypeReference.
        /// </summary>
        /// <param name="allReferences"></param>
        private MultipleTypeReference(ImmutableHashSet<ITypeReference> allReferences)
        {
            AllReferences = allReferences.ToImmutableHashSet();
        }

        /// <summary>
        /// The type references.
        /// </summary>
        public ImmutableHashSet<ITypeReference> AllReferences { get; }

        /// <inheritdoc />
        public bool Equals(ITypeReference? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other switch
            {
                ActualTypeReference actualType => AllReferences.Count == 1 && AllReferences.Contains(actualType),
                MultipleTypeReference multipleTypeReference => AllReferences.SetEquals(multipleTypeReference
                    .AllReferences),
                VariableTypeReference variableTypeReference => AllReferences.Count == 1 && AllReferences.Contains(variableTypeReference),
                GenericTypeReference genericTypeReference => AllReferences.Count == 1 && AllReferences.Contains(genericTypeReference),
                _ => throw new ArgumentOutOfRangeException(nameof(other))
            };
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ITypeReference other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => AllReferences.FirstOrDefault().GetHashCode();

        /// <inheritdoc />
        public IEnumerable<VariableTypeReference> VariableTypeReferences => AllReferences.SelectMany(x=>x.VariableTypeReferences);

        /// <inheritdoc />
        public IEnumerable<ActualTypeReference> ActualTypeReferences => AllReferences.SelectMany(x=>x.ActualTypeReferences);

        /// <inheritdoc />
        public IEnumerable<ITypeReference> TypeArgumentReferences => ImmutableList<ITypeReference>.Empty;
    }
}