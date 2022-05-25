using OneOf.Types;
using Reductech.Sequence.Core.Internal.Documentation;

namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A reference to a type
/// </summary>
public abstract record TypeReference
{
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
    /// The name of the type in human readable form
    /// </summary>
    public abstract string HumanReadableTypeName { get; }

    /// <summary>
    /// Create a schema node which accepts objects of this type
    /// </summary>
    /// <returns></returns>
    public abstract SchemaNode ToSchemaNode(TypeResolver typeResolver);

    /// <summary>
    /// Try to combine this type reference with another
    /// </summary>
    public Result<TypeReference, IErrorBuilder> TryCombine(
        TypeReference other,
        TypeResolver? typeResolver)
    {
        if (this is Unknown or Any)
            return other;

        if (other is Unknown or Any)
            return this;

        if (this is Array thisArray)
        {
            if (other is Dynamic or Any)
            {
                var result = thisArray.MemberType.TryCombine(other, typeResolver)
                    .Map(x => new Array(x) as TypeReference);

                return result;
            }

            if (other is Array otherArray)
            {
                var result = thisArray.MemberType.TryCombine(otherArray.MemberType, typeResolver)
                    .Map(x => new Array(x) as TypeReference);

                return result;
            }
        }
        else if (other is Array) //Flip to get the array logic in the other direction
        {
            return other.TryCombine(this, typeResolver);
        }

        if (this is Entity e1 && other is Entity e2)
        {
            if (e2.Schema.HasNoValue)
                return e1;

            if (e1.Schema.HasNoValue)
                return e2;

            var combined = (EntityNode)e1.Schema.Value.Combine(e2.Schema.Value);

            return new Entity(combined);
        }

        if (other.Allow(this, typeResolver))
            return this;

        if (Allow(other, typeResolver))
            return other; //Return the more restricted type

        return ErrorCode.TypesIncompatible.ToErrorBuilder(Name, other.Name);
    }

    /// <summary>
    /// The name of this type
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Is this unknown or a array of unknown
    /// </summary>
    public virtual bool IsUnknown => false;

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
            return typeof(ISCLObject);
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            return true;
        }

        /// <inheritdoc />
        public override string Name => nameof(Any);

        /// <inheritdoc />
        public override string HumanReadableTypeName => "any";

        /// <inheritdoc />
        public override SchemaNode ToSchemaNode(TypeResolver typeResolver) => TrueNode.Instance;
    }

    /// <summary>
    /// A particular type
    /// </summary>
    public abstract record Actual : TypeReference
    {
        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver) =>
            ErrorCode.CannotInferType.ToErrorBuilder($"{Name} is not an Array Type");

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            other = typeResolver?.MaybeResolve(other) ?? other;

            if (Equals(other))
                return true;

            return false;
        }

        /// <summary>
        /// A string
        /// </summary>
        public static Actual String { get; } = new TypedActual<StringStream>(
            new StringNode(
                EnumeratedValuesNodeData.Empty,
                AnyStringFormat.Instance,
                StringRestrictions.NoRestrictions
            )
        );

        /// <summary>
        /// An integer
        /// </summary>
        public static Actual Integer { get; } = new TypedActual<SCLInt>(
            new IntegerNode(EnumeratedValuesNodeData.Empty, NumberRestrictions.NoRestrictions)
        );

        /// <summary>
        /// A double
        /// </summary>
        public static Actual Double { get; } = new TypedActual<SCLDouble>(
            new NumberNode(EnumeratedValuesNodeData.Empty, NumberRestrictions.NoRestrictions)
        );

        /// <summary>
        /// A boolean
        /// </summary>
        public static Actual Bool { get; } =
            new TypedActual<SCLBool>(new BooleanNode(EnumeratedValuesNodeData.Empty));

        /// <summary>
        /// A date
        /// </summary>
        public static Actual Date { get; } = new TypedActual<SCLDateTime>(
            new StringNode(
                EnumeratedValuesNodeData.Empty,
                DateTimeStringFormat.Instance,
                StringRestrictions.NoRestrictions
            )
        );

        /// <summary>
        /// A null value
        /// </summary>
        public static Actual Null { get; } = new TypedActual<SCLNull>(NullNode.Instance);

        private sealed record TypedActual<T>(SchemaNode SchemaNode) : Actual where T : ISCLObject
        {
            /// <inheritdoc />
            public override string Name => typeof(T).Name;

            /// <param name="typeResolver"></param>
            /// <inheritdoc />
            public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver) =>
                typeof(T);

            /// <inheritdoc />
            public override string HumanReadableTypeName { get; } =
                TypeNameHelper.GetHumanReadableTypeName(typeof(T));

            /// <inheritdoc />
            public override SchemaNode ToSchemaNode(TypeResolver typeResolver) => SchemaNode;
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
            TypeResolver typeResolver) =>
            ErrorCode.CannotInferType.ToErrorBuilder($"Unit is not an Array Type");

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
        public override SchemaNode ToSchemaNode(TypeResolver typeResolver) => NullNode.Instance;

        /// <inheritdoc />
        public override string Name => nameof(Unit);

        /// <inheritdoc />
        public override string HumanReadableTypeName { get; } =
            TypeNameHelper.GetHumanReadableTypeName(typeof(Unit));
    }

    /// <summary>
    /// An entity type reference
    /// </summary>
    public sealed record Entity(Maybe<EntityNode> Schema) : TypeReference
    {
        /// <summary>
        /// An entity type with no schema defined
        /// </summary>
        public static Entity NoSchema { get; } = new(Maybe<EntityNode>.None);

        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver)
        {
            return ErrorCode.CannotInferType.ToErrorBuilder($"{Name} is not an Array Type");
        }

        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            return typeof(Core.Entity);
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            other = typeResolver?.MaybeResolve(other) ?? other;

            if (other is not Entity otherAsEntity)
                return false;

            if (Schema.HasNoValue || otherAsEntity.Schema.HasNoValue)
                return true;

            if (Schema.Value.IsSuperset(otherAsEntity.Schema.Value))
                return true;

            return false;
        }

        /// <inheritdoc />
        public override string HumanReadableTypeName => nameof(Core.Entity);

        /// <inheritdoc />
        public override SchemaNode ToSchemaNode(TypeResolver typeResolver)
        {
            if (Schema.HasValue)
                return Schema.Value;

            return TrueNode.Instance;
        }

        /// <inheritdoc />
        public override string Name => nameof(Core.Entity);
    }

    /// <summary>
    /// An enum type
    /// </summary>
    public sealed record Enum(Type EnumType) : TypeReference
    {
        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver) =>
            typeof(SCLEnum<>).MakeGenericType(EnumType);

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
            TypeResolver typeResolver) =>
            ErrorCode.CannotInferType.ToErrorBuilder($"{EnumType.Name} is not an Array Type");

        /// <inheritdoc />
        public override string HumanReadableTypeName { get; } =
            TypeNameHelper.GetHumanReadableTypeName(EnumType);

        /// <inheritdoc />
        public override SchemaNode ToSchemaNode(TypeResolver typeResolver)
        {
            var instance =
                (ISCLEnum)Activator.CreateInstance(typeof(SCLEnum<>).MakeGenericType(EnumType))!;

            var values = instance.GetAllValues().ToList();

            return new StringNode(
                new EnumeratedValuesNodeData(values),
                AnyStringFormat.Instance,
                StringRestrictions.NoRestrictions
            );
        }
    }

    /// <summary>
    /// A discriminated union type
    /// </summary>
    public sealed record OneOf(TypeReference[] Options) : TypeReference
    {
        /// <inheritdoc />
        public override string Name => "OneOf" + string.Join("Or", Options.Select(x => x.Name));

        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver)
        {
            foreach (var typeReference in Options)
            {
                var r = typeReference.TryGetArrayMemberTypeReference(typeResolver);

                if (r.IsSuccess)
                    return r;
            }

            return ErrorCode.CannotInferType.ToErrorBuilder($"OneOf is not an Array Type");
        }

        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            var types = new HashSet<Type>();

            foreach (var typeReference in Options)
            {
                var t = typeReference.TryGetType(typeResolver);

                if (t.IsFailure)
                    return t.ConvertFailure<Type>();

                if (t.Value != typeof(ISCLObject))
                    types.Add(t.Value);
            }

            Type genericOneOfType;

            if (types.Count == 0)
                return typeof(ISCLObject);

            if (types.Count == 1)
                return types.Single();

            if (types.Count == 2)
            {
                if (types.Contains(typeof(SCLInt)) && types.Contains(typeof(SCLDouble)))
                    return typeof(SCLDouble);

                genericOneOfType = typeof(SCLOneOf<,>);
            }

            else if (types.Count == 3)
                genericOneOfType = typeof(SCLOneOf<,,>);
            else if (types.Count == 4)
                genericOneOfType = typeof(SCLOneOf<,,,>);
            else
                throw new Exception($"Cannot create a OneOf with {types.Count} type arguments");

            var actualOneOfType = genericOneOfType.MakeGenericType(types.ToArray());

            return actualOneOfType;
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            if (other is OneOf otherOneOf)
            {
                foreach (var otherOption in otherOneOf.Options)
                {
                    if (!Allow(otherOption, typeResolver))
                        return false;
                }

                return true;
            }

            return Options.Any(x => x.Allow(other, typeResolver));
        }

        /// <inheritdoc />
        public override bool IsUnknown => Options.Any(x => x.IsUnknown);

        /// <inheritdoc />
        public override string HumanReadableTypeName { get; } = string.Join(
            " or ",
            Options.Select(x => x.HumanReadableTypeName)
        );

        /// <inheritdoc />
        public override SchemaNode ToSchemaNode(TypeResolver typeResolver)
        {
            return new OneOfNode(
                new OneOfNodeData(
                    Options.Select(x => x.ToSchemaNode(typeResolver)).Distinct().ToArray()
                )
            );
        }
    }

    /// <summary>
    /// A dynamic type - the result of getting the value from an entity
    /// </summary>
    public sealed record Dynamic : TypeReference
    {
        private Dynamic() { }

        /// <summary>
        /// The instance of the the Dynamic Type
        /// </summary>
        public static Dynamic Instance { get; } = new();

        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver)
        {
            return Instance;
        }

        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            return typeof(ISCLObject);
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            return true;
        }

        /// <inheritdoc />
        public override SchemaNode ToSchemaNode(TypeResolver typeResolver) => TrueNode.Instance;

        /// <inheritdoc />
        public override string Name => "Dynamic";

        /// <inheritdoc />
        public override string HumanReadableTypeName { get; } = "dynamic";
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
        public override SchemaNode ToSchemaNode(TypeResolver typeResolver)
        {
            var member = MemberType.ToSchemaNode(typeResolver);

            return new ArrayNode(
                EnumeratedValuesNodeData.Empty,
                new ItemsData(ArraySegment<SchemaNode>.Empty, member)
            );
        }

        /// <inheritdoc />
        public override string Name => $"ArrayOf{MemberType.Name}";

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<TypeReference, IErrorBuilder> TryGetArrayMemberTypeReference(
            TypeResolver typeResolver)
        {
            return MemberType;
        }

        /// <inheritdoc />
        public override bool IsUnknown => MemberType.IsUnknown;

        /// <inheritdoc />
        public override string HumanReadableTypeName
        {
            get
            {
                var member = MemberType.HumanReadableTypeName;

                return $"`array<{member.Trim('`')}>`";
            }
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

            var variable = new Variable(typeResolver.AutomaticVariableName.GetValueOrThrow());

            return variable.TryGetArrayMemberTypeReference(typeResolver);
        }

        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            if (typeResolver.AutomaticVariableName.HasNoValue)
                return ErrorCode.AutomaticVariableNotSet.ToErrorBuilder();

            var variable = new Variable(typeResolver.AutomaticVariableName.GetValueOrThrow());

            return variable.TryGetType(typeResolver);
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            if (typeResolver is null || typeResolver.AutomaticVariableName.HasNoValue)
                return false;

            var variable = new Variable(typeResolver.AutomaticVariableName.GetValueOrThrow());

            return variable.Allow(other, typeResolver);
        }

        /// <inheritdoc />
        public override SchemaNode ToSchemaNode(TypeResolver typeResolver)
        {
            if (typeResolver.AutomaticVariableName.HasNoValue)
                return TrueNode.Instance;

            if (typeResolver.Dictionary.TryGetValue(
                    typeResolver.AutomaticVariableName.GetValueOrThrow(),
                    out var tr
                ))
            {
                return tr.ToSchemaNode(typeResolver);
            }

            return TrueNode.Instance;
        }

        /// <inheritdoc />
        public override string Name => nameof(AutomaticVariable);

        /// <inheritdoc />
        public override string HumanReadableTypeName { get; } = "`variable name`";
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
            var vtr = this;

            HashSet<TypeReference> typeReferences = new(); //prevent circular references

            while (typeReferences.Add(vtr)
                && typeResolver.Dictionary.TryGetValue(vtr.VariableName, out var tr))
            {
                if (tr is Variable vtr2)
                    vtr = vtr2;

                else
                    return tr.TryGetArrayMemberTypeReference(typeResolver);
            }

            return ErrorCode.CannotInferType.ToErrorBuilder($"{VariableName} is not an Array Type");
        }

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public override Result<Type, IErrorBuilder> TryGetType(TypeResolver typeResolver)
        {
            var vtr = this;

            HashSet<TypeReference> typeReferences = new(); //prevent circular references

            while (typeReferences.Add(vtr)
                && typeResolver.Dictionary.TryGetValue(vtr.VariableName, out var tr))
            {
                if (tr is Variable vtr2)
                    vtr = vtr2;

                else
                    return tr.TryGetType(typeResolver);
            }

            return ErrorCode.CannotInferType.ToErrorBuilder(
                $"{VariableName} could not be inferred"
            );
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
        public override SchemaNode ToSchemaNode(TypeResolver typeResolver)
        {
            if (typeResolver.Dictionary.TryGetValue(
                    VariableName,
                    out var tr
                ))
            {
                return tr.ToSchemaNode(typeResolver);
            }

            return TrueNode.Instance;
        }

        /// <inheritdoc />
        public override string Name => VariableName.Serialize(SerializeOptions.Serialize);

        /// <inheritdoc />
        public override string HumanReadableTypeName { get; } = "`variable name`";
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
            return ErrorCode.CannotInferType.ToErrorBuilder($"Unknown type");
        }

        /// <inheritdoc />
        public override bool Allow(TypeReference other, TypeResolver? typeResolver)
        {
            return true;
        }

        /// <inheritdoc />
        public override SchemaNode ToSchemaNode(TypeResolver typeResolver) => TrueNode.Instance;

        /// <inheritdoc />
        public override string Name => nameof(Unknown);

        /// <inheritdoc />
        public override bool IsUnknown => true;

        /// <inheritdoc />
        public override string HumanReadableTypeName { get; } = "`unknown`";
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

        if (stepType.IsGenericType
         && stepType.GetGenericTypeDefinition() == typeof(LambdaFunction<,>))
        {
            var nested = stepType.GenericTypeArguments[1];
            return Create(nested);
        }

        if (stepType == typeof(IStep))
        {
            return Any.Instance;
        }

        if (stepType == typeof(VariableName))
            return Unknown.Instance;

        throw new Exception($"Type '{stepType}' was not a step type");
    }

    /// <summary>
    /// Create a type reference from a type
    /// </summary>
    public static TypeReference Create(Type t)
    {
        if (t.IsGenericType)
        {
            var genericTypeDefinition = t.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(Array<>) ||
                genericTypeDefinition == typeof(EagerArray<>) ||
                genericTypeDefinition == typeof(LazyArray<>))
            {
                var nested          = t.GenericTypeArguments[0];
                var nestedReference = Create(nested);
                return new Array(nestedReference);
            }

            if (t.GetInterfaces().Contains(typeof(ISCLOneOf)))
            {
                var nestedTypeReferences =
                    t.GenericTypeArguments.Select(Create).ToArray();

                return new OneOf(nestedTypeReferences);
            }

            if (t.GetInterfaces().Contains(typeof(ISCLEnum)))
            {
                var enumType = t.GenericTypeArguments.Single();
                return new Enum(enumType);
            }
        }

        return t.Name switch
        {
            nameof(StringStream) => Actual.String,
            nameof(SCLInt)       => Actual.Integer,
            nameof(SCLDouble)    => Actual.Double,
            nameof(SCLBool)      => Actual.Bool,
            nameof(SCLDateTime)  => Actual.Date,
            nameof(SCLNull)      => Actual.Null,
            nameof(Unit)         => Unit.Instance,
            nameof(Entity)       => Entity.NoSchema,
            nameof(ISCLObject)   => Any.Instance,
            _                    => Unknown.Instance
        };
    }

    /// <summary>
    /// Disambiguate a type reference
    /// </summary>
    public static TypeReference Create(IEnumerable<TypeReference> typeReference)
    {
        var references = typeReference.Distinct().ToArray();

        if (references.Length == 0)
            return Any.Instance;

        if (references.Length == 1)
            return references.Single();

        return new OneOf(references);
    }
}
