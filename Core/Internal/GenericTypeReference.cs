using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
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
        ChildTypes  = childTypes;
    }

    /// <inheritdoc />
    public IEnumerable<VariableTypeReference> VariableTypeReferences =>
        ImmutableList<VariableTypeReference>.Empty;

    /// <inheritdoc />
    public Result<ActualTypeReference, IErrorBuilder> TryGetActualTypeReference(
        TypeResolver typeResolver)
    {
        var result = ChildTypes
            .Select(ct => ct.TryGetActualTypeReference(typeResolver))
            .Combine(ErrorBuilderList.Combine)
            .Bind(x => Create(GenericType, x));

        return result;

        static Result<ActualTypeReference, IErrorBuilder> Create(
            Type genericType,
            IEnumerable<ActualTypeReference> actualTypeReferences)
        {
            var arguments = actualTypeReferences.Select(x => x.Type).ToArray();

            try
            {
                var t = genericType.MakeGenericType(arguments);

                return new ActualTypeReference(t);
            }
            #pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return new ErrorBuilder(e, ErrorCode.InvalidCast);
            }
            #pragma warning restore CA1031 // Do not catch general exception types
        }
    }

    /// <inheritdoc />
    public Result<ActualTypeReference, IErrorBuilder> TryGetGenericTypeReference(
        TypeResolver typeResolver,
        int argumentNumber)
    {
        if (argumentNumber < 0 || TypeArgumentReferences.Count <= argumentNumber)
            return new ErrorBuilder(ErrorCode.CannotInferType);

        var r = TypeArgumentReferences[argumentNumber].TryGetActualTypeReference(typeResolver);

        return r;
    }

    /// <inheritdoc />
    public Result<Maybe<ActualTypeReference>, IErrorBuilder> GetActualTypeReferenceIfResolvable(
        TypeResolver typeResolver)
    {
        var result = ChildTypes
            .Select(ct => ct.GetActualTypeReferenceIfResolvable(typeResolver))
            .Combine(ErrorBuilderList.Combine)
            .Bind(x => Create(GenericType, x));

        return result;

        static Result<Maybe<ActualTypeReference>, IErrorBuilder> Create(
            Type genericType,
            IEnumerable<Maybe<ActualTypeReference>> actualTypeReferences)
        {
            var arguments = new List<Type>();

            foreach (var actualTypeReference in actualTypeReferences)
            {
                if (actualTypeReference.HasValue)
                    arguments.Add(actualTypeReference.Value.Type);
                else
                    return Maybe<ActualTypeReference>.None;
            }

            try
            {
                var t = genericType.MakeGenericType(arguments.ToArray());

                return Maybe<ActualTypeReference>.From(new ActualTypeReference(t));
            }
            #pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return new ErrorBuilder(e, ErrorCode.InvalidCast);
            }
            #pragma warning restore CA1031 // Do not catch general exception types
        }
    }

    /// <summary>
    /// The generic type references
    /// </summary>
    public IReadOnlyList<ITypeReference> TypeArgumentReferences => ChildTypes;

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
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return other is GenericTypeReference gtr && GenericType == gtr.GenericType &&
               ChildTypes.SequenceEqual(gtr.ChildTypes);
    }
}

}
