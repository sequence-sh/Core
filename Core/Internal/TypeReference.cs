using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A reference to a type
/// </summary>
public abstract record TypeReference
{
    /// <summary>
    /// The type could be any type
    /// </summary>
    public sealed record Any : TypeReference
    {
        private Any() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static Any Instance { get; } = new();

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver)
        {
            return Unknown.Instance;
        }

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            return typeof(object);
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            return true;
        }

        /// <inheritdoc />
        public override string Name => nameof(Any);
    }

    /// <summary>
    /// A particular type
    /// </summary>
    public sealed record Actual : TypeReference
    {
        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver) => ErrorCode.CannotInferType.ToErrorBuilder();

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver) =>
            Type.GetCSharpType();

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            other = typeResolver?.MaybeResolve(other) ?? other;

            if (other is Actual a)
            {
                if (a.Type == Type)
                    return true;

                //if (Type == SCLType.Double || a.Type == SCLType.Integer)
                //    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override string Name => Type.ToString();

        private Actual(SCLType type) => Type = type;

        /// <summary>
        /// The expected type
        /// </summary>
        public SCLType Type { get; }

        /// <summary>
        /// A string
        /// </summary>
        public static Actual String { get; } = new(SCLType.String);

        /// <summary>
        /// An integer
        /// </summary>
        public static Actual Integer { get; } = new(SCLType.Integer);

        /// <summary>
        /// A double
        /// </summary>
        public static Actual Double { get; } = new(SCLType.Double);

        /// <summary>
        /// A boolean
        /// </summary>
        public static Actual Bool { get; } = new(SCLType.Bool);

        /// <summary>
        /// A date
        /// </summary>
        public static Actual Date { get; } = new(SCLType.Date);

        /// <summary>
        /// An entity
        /// </summary>
        public static Actual Entity { get; } = new(SCLType.Entity);

        /// <summary>
        /// Create an actual type.
        /// This does not work for Enum types
        /// </summary>
        internal static Actual Create(SCLType type)
        {
            return type switch
            {
                SCLType.String  => String,
                SCLType.Integer => Integer,
                SCLType.Double  => Double,
                SCLType.Bool    => Bool,
                SCLType.Date    => Date,
                SCLType.Entity  => Entity,
                SCLType.Enum => throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Cannot convert enum type to actual"
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }

    /// <summary>
    /// A reference of the Unit type
    /// </summary>
    public sealed record Unit : TypeReference
    {
        private Unit() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static Unit Instance { get; } = new();

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver) => ErrorCode.CannotInferType.ToErrorBuilder();

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            return typeof(Util.Unit);
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            other = typeResolver?.MaybeResolve(other) ?? other;

            return other is Unit;
        }

        /// <inheritdoc />
        public override string Name => nameof(Unit);
    }

    /// <summary>
    /// An enum type
    /// </summary>
    public sealed record Enum(Type EnumType) : TypeReference
    {
        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            return EnumType;
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            other = typeResolver?.MaybeResolve(other) ?? other;

            return other is Enum e && EnumType == e.EnumType;
        }

        /// <inheritdoc />
        public override string Name => EnumType.Name;

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver) => ErrorCode.CannotInferType.ToErrorBuilder();
    }

    /// <summary>
    /// An array of the type
    /// </summary>
    public sealed record Array(TypeReference MemberType) : TypeReference
    {
        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            var r = MemberType.TryGetType(typeResolver)
                    .Map(x => typeof(Array<>).MakeGenericType(x))
                ;

            return r;
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            other = typeResolver?.MaybeResolve(other) ?? other;

            return other is Array array && MemberType.Allow(array.MemberType, typeResolver);
        }

        /// <inheritdoc />
        public override string Name => $"Array<{MemberType.Name}>";

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver)
        {
            return MemberType;
        }
    }

    /// <summary>
    /// One of several types
    /// </summary>
    public sealed record Multiple(IReadOnlySet<TypeReference> Options) : TypeReference
    {
        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            if (Options.Count == 1)
                return Options.Single().TryGetType(typeResolver);

            var possibleTypes = Options.Where(x => x != Any.Instance)
                .Select(x => x.TryGetType(typeResolver))
                .Combine(ErrorBuilderList.Combine)
                .Map(x => x.Distinct().Where(t => t != typeof(object)).ToList());

            if (possibleTypes.IsFailure)
                return possibleTypes.ConvertFailure<Type>();

            if (possibleTypes.Value.Count == 0)
                return typeof(object);

            if (possibleTypes.Value.Count == 1)
                return possibleTypes.Value.Single();

            if (possibleTypes.Value.Count == 2)
            {
                if (possibleTypes.Value.Contains(typeof(double))
                 && possibleTypes.Value.Contains(typeof(int)))
                    return typeof(double);
            }

            return ErrorCode.CannotInferType.ToErrorBuilder();
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            other = typeResolver?.MaybeResolve(other) ?? other;

            return Options.Any(x => x.Allow(other, typeResolver));
        }

        /// <inheritdoc />
        public override string Name => string.Join(" or ", Options.Select(x => x.Name));

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver)
        {
            if (Options.Count == 1)
                return Options.Single().TryGetArrayMemberTypeReference(typeResolver);

            return ErrorCode.CannotInferType.ToErrorBuilder();
        }
    }

    /// <summary>
    /// The automatic variable
    /// </summary>
    public sealed record AutomaticVariable : TypeReference
    {
        private AutomaticVariable() { }

        /// <summary>
        /// The Automatic Variable instance
        /// </summary>
        public static AutomaticVariable Instance { get; } = new();

        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver)
        {
            if (typeResolver.AutomaticVariableName.HasNoValue)
                return ErrorCode.AutomaticVariableNotSet.ToErrorBuilder();

            var variable = new Variable(typeResolver.AutomaticVariableName.Value);

            return variable.TryGetArrayMemberTypeReference(typeResolver);
        }

        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            if (typeResolver.AutomaticVariableName.HasNoValue)
                return ErrorCode.AutomaticVariableNotSet.ToErrorBuilder();

            var variable = new Variable(typeResolver.AutomaticVariableName.Value);

            return variable.TryGetType(typeResolver);
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            if (typeResolver is null || typeResolver.AutomaticVariableName.HasNoValue)
                return false;

            var variable = new Variable(typeResolver.AutomaticVariableName.Value);

            return variable.Allow(other, typeResolver);
        }

        /// <inheritdoc />
        public override string Name => nameof(AutomaticVariable);
    }

    /// <summary>
    /// A variable reference
    /// </summary>
    public sealed record Variable(VariableName VariableName) : TypeReference
    {
        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver)
        {
            Variable vtr = this;

            HashSet<TypeReference> typeReferences = new(); //prevent circular references

            while (typeReferences.Add(vtr)
                && typeResolver.Dictionary.TryGetValue(vtr.VariableName, out var tr))
            {
                if (tr is Variable vtr2)
                    vtr = vtr2;

                else
                    return tr.TryGetArrayMemberTypeReference(typeResolver);
            }

            return ErrorCode.CannotInferType.ToErrorBuilder();
        }

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            Variable vtr = this;

            HashSet<TypeReference> typeReferences = new(); //prevent circular references

            while (typeReferences.Add(vtr)
                && typeResolver.Dictionary.TryGetValue(vtr.VariableName, out var tr))
            {
                if (tr is Variable vtr2)
                    vtr = vtr2;

                else
                    return tr.TryGetType(typeResolver);
            }

            return ErrorCode.CannotInferType.ToErrorBuilder();
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            if (typeResolver is null
             || !typeResolver.Dictionary.TryGetValue(VariableName, out var resolvedType))
                return true;

            return resolvedType.Allow(other, typeResolver);
        }

        /// <inheritdoc />
        public override string Name => VariableName.Serialize();
    }

    /// <summary>
    /// Type is unknown
    /// </summary>
    public sealed record Unknown : TypeReference
    {
        private Unknown() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static Unknown Instance { get; } = new();

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver)
        {
            return Instance;
        }

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            return ErrorCode.CannotInferType.ToErrorBuilder();
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            return true;
        }

        /// <inheritdoc />
        public override string Name => nameof(Unknown);
    }

    /// <summary>
    /// Create a type reference from a step type
    /// </summary>
    public static TypeReference CreateFromStepType(Type stepType)
    {
        if (stepType.IsGenericType && stepType.GetGenericTypeDefinition() == typeof(IStep<>))
        {
            var nested = stepType.GenericTypeArguments[0];
            return Create(nested);
        }
        else if (stepType == typeof(IStep))
        {
            return Any.Instance;
        }

        throw new Exception("Type was not a step type");
    }

    /// <summary>
    /// Create a type reference from a type
    /// </summary>
    public static TypeReference Create(Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Array<>))
        {
            var nested = t.GenericTypeArguments[0];

            var nestedReference = Create(nested);
            return new Array(nestedReference);
        }

        var sclType = t.GetSCLType();

        return sclType switch
        {
            null when t == typeof(object)    => Any.Instance,
            null when t == typeof(Util.Unit) => Unit.Instance,
            null                             => Unknown.Instance,
            SCLType.Enum                     => new Enum(t),
            _                                => Actual.Create(sclType.Value)
        };
    }

    /// <summary>
    /// Disambiguate a type reference
    /// </summary>
    public static TypeReference Create(IEnumerable<TypeReference> typeReference)
    {
        var references = typeReference.ToHashSet();

        if (references.Count == 0)
            return Any.Instance;

        if (references.Count == 1)
            return references.Single();

        return new Multiple(references);
    }

    /// <summary>
    /// Gets the array member type if available
    /// </summary>
    /// <param name="typeResolver"></param>
    /// <returns></returns>
    public abstract Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
        TypeResolver typeResolver);

    /// <summary>
    /// Gets the type referred to by a reference.
    /// </summary>
    /// <param name="typeResolver"></param>
    public abstract Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver);

    /// <summary>
    /// Whether this allows the other type reference
    /// </summary>
    public abstract bool Allow(TypeReference other, TypeResolver? typeResolver);

    /// <summary>
    /// The name of this type
    /// </summary>
    public abstract string Name { get; }
}

}
