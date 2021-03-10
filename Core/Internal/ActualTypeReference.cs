//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using CSharpFunctionalExtensions;
//using Namotion.Reflection;
//using Reductech.EDR.Core.Internal.Errors;

//namespace Reductech.EDR.Core.Internal
//{

//public sealed 

///// <summary>
///// An actual instance of the type.
///// </summary>
//public sealed class ActualTypeReference : ITypeReference, IEquatable<ITypeReference>
//{
//    /// <summary>
//    /// Creates a new ActualTypeReference.
//    /// </summary>
//    public ActualTypeReference(Type type) => Type = type;

//    /// <summary>
//    /// The type to use.
//    /// </summary>
//    public Type Type { get; }

//    /// <inheritdoc />
//    public bool Equals(ITypeReference? other)
//    {
//        if (other is null)
//            return false;

//        if (ReferenceEquals(this, other))
//            return true;

//        return other switch
//        {
//            ActualTypeReference actualType => Type == actualType.Type,
//            MultipleTypeReference multipleTypeReference => multipleTypeReference.AllReferences.Count
//                                                        == 0 &&
//                                                           multipleTypeReference.AllReferences
//                                                               .Contains(this),
//            VariableTypeReference _ => false,
//            GenericTypeReference _  => false,
//            _                       => throw new ArgumentOutOfRangeException(nameof(other))
//        };
//    }

//    /// <inheritdoc />
//    public override string ToString() => Type.GetDisplayName();

//    /// <inheritdoc />
//    public override bool Equals(object? obj) =>
//        ReferenceEquals(this, obj) || obj is ITypeReference other && Equals(other);

//    /// <inheritdoc />
//    public override int GetHashCode() => Type.GetHashCode();

//    /// <inheritdoc />
//    IEnumerable<VariableTypeReference> ITypeReference.VariableTypeReferences =>
//        ImmutableArray<VariableTypeReference>.Empty;

//    /// <inheritdoc />
//    public bool AllowType(Type type)
//    {
//        return Type == type;
//    }

//    /// <inheritdoc />
//    public Result<ActualTypeReference, IErrorBuilder> TryGetActualTypeReference(TypeResolver _) =>
//        this;

//    /// <inheritdoc />
//    public Result<ActualTypeReference, IErrorBuilder> TryGetGenericTypeReference(
//        TypeResolver typeResolver,
//        int argumentNumber)
//    {
//        if (!Type.IsGenericType)
//            return new ErrorBuilder(ErrorCode.InvalidCast, Type.Name, "Generic Type");

//        if (argumentNumber < 0 || Type.GenericTypeArguments.Length <= argumentNumber)
//            return new ErrorBuilder(ErrorCode.IndexOutOfBounds);

//        var t = Type.GenericTypeArguments[argumentNumber];
//        return new ActualTypeReference(t);
//    }

//    /// <inheritdoc />
//    public Result<Maybe<ActualTypeReference>, IErrorBuilder>
//        GetActualTypeReferenceIfResolvable(TypeResolver typeResolver) =>
//        TryGetActualTypeReference(typeResolver).Map(Maybe<ActualTypeReference>.From);

//    /// <summary>
//    /// Creates a fixed type reference from a type
//    /// </summary>
//    public static ITypeReference Create(Type type)
//    {
//        if (type.IsGenericType)
//        {
//            var genericTypeDef = type.GetGenericTypeDefinition();
//            var arguments      = type.GenericTypeArguments;

//            return new GenericTypeReference(genericTypeDef, arguments.Select(Create).ToList());
//        }
//        else
//            return new ActualTypeReference(type);
//    }
//}

//}


