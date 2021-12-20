//using System.Runtime.Serialization;
//using System.Text.Json.Serialization;

//namespace Reductech.Sequence.Core.Entities;

///// <summary>
///// The value of an entity property.
///// </summary>
//public abstract record EntityValue(object ObjectValue)
//{
//    /// <summary>
//    /// The Null value
//    /// </summary>
//    public record Null : ISCLObject
//    {
//        private Null() : base(SCLNull.Instance) { }

//        /// <summary>
//        /// The instance
//        /// </summary>
//        public static Null Instance { get; } = new();

//        /// <inheritdoc />
//        public override string Serialize() => "null";

//        /// <inheritdoc />
//        public override string Serialize() => Serialized;

//        private static readonly string Serialized = "null";

//        /// <inheritdoc />
//        public override string GetFormattedString(
//            char delimiter,
//            string dateTimeFormat) => "null";

//        /// <inheritdoc />
//        protected override Maybe<object> AsType(Type type)
//        {
//            return Maybe<object>.None;
//        }

//        /// <inheritdoc />
//        public override SchemaNode ToSchemaNode(
//            string path,
//            SchemaConversionOptions? schemaConversionOptions) => NullNode.Instance;

//        /// <inheritdoc />
//        public override string ToString() => Serialize();
//    }

//    /// <summary>
//    /// A string value
//    /// </summary>
//    public record String(string Value) : ISCLObject(Value)
//    {
//        /// <inheritdoc />
//        public override string Serialize()
//        {
//            return Value;
//        }

//        /// <inheritdoc />
//        public override string Serialize()
//        {
//            return SerializationMethods.DoubleQuote(Value);
//        }

//        /// <inheritdoc />
//        public override string GetFormattedString(
//            char delimiter,
//            string dateTimeFormat) => Value;

//        /// <inheritdoc />
//        protected override Maybe<object> AsType(Type type)
//        {
//            if (type == typeof(string))
//                return Value;

//            if (type == typeof(StringStream) || type == typeof(object))
//                return new StringStream(Value);

//            return Maybe<object>.None;
//        }

//        /// <inheritdoc />
//        public override SchemaNode ToSchemaNode(
//            string path,
//            SchemaConversionOptions? schemaConversionOptions)
//        {
//            if (schemaConversionOptions is null)
//                return StringNode.Default;

//            return schemaConversionOptions.GetNode(Value, path);
//        }

//        /// <inheritdoc />
//        public override string ToString() => Serialize();
//    }

//    /// <summary>
//    /// An integer value
//    /// </summary>
//    public record Integer(int Value) : ISCLObject(Value)
//    {
//        /// <inheritdoc />
//        public override string GetFormattedString(
//            char delimiter,
//            string dateTimeFormat) => Value.ToString();

//        /// <inheritdoc />
//        public override string Serialize()
//        {
//            return Value.ToString();
//        }

//        /// <inheritdoc />
//        public override string Serialize() => Value.ToString();

//        /// <inheritdoc />
//        protected override Maybe<object> AsType(Type type)
//        {
//            if (type == typeof(int) || type == typeof(Double))
//                return Value;

//            return Maybe<object>.None;
//        }

//        /// <inheritdoc />
//        public override SchemaNode ToSchemaNode(
//            string path,
//            SchemaConversionOptions? schemaConversionOptions)
//        {
//            return IntegerNode.Default;
//        }

//        /// <inheritdoc />
//        public override string ToString() => Serialize();
//    }

//    /// <summary>
//    /// A double precision floating point value
//    /// </summary>
//    public record Double(double Value) : ISCLObject(Value)
//    {
//        /// <inheritdoc />
//        public override string GetFormattedString(
//            char delimiter,
//            string dateTimeFormat) => Value.ToString(Constants.DoubleFormat);

//        /// <inheritdoc />
//        public override string Serialize() => Value.ToString("R");

//        /// <inheritdoc />
//        public override string Serialize() => Value.ToString(Constants.DoubleFormat);

//        /// <inheritdoc />
//        protected override Maybe<object> AsType(Type type)
//        {
//            if (type == typeof(double))
//                return Value;

//            return Maybe<object>.None;
//        }

//        /// <inheritdoc />
//        public override SchemaNode ToSchemaNode(
//            string path,
//            SchemaConversionOptions? schemaConversionOptions)
//        {
//            return NumberNode.Default;
//        }

//        /// <inheritdoc />
//        public override string ToString() => Serialize();
//    }

//    /// <summary>
//    /// A boolean value
//    /// </summary>
//    public record Boolean(bool Value) : ISCLObject(Value)
//    {
//        /// <inheritdoc />
//        public override SchemaNode ToSchemaNode(
//            string path,
//            SchemaConversionOptions? schemaConversionOptions)
//        {
//            return BooleanNode.Default;
//        }

//        /// <inheritdoc />
//        public override string Serialize() => Value.ToString();

//        /// <inheritdoc />
//        public override string Serialize() => Value.ToString();

//        /// <inheritdoc />
//        public override string GetFormattedString(
//            char delimiter,
//            string dateTimeFormat) => Value.ToString();

//        /// <inheritdoc />
//        protected override Maybe<object> AsType(Type type)
//        {
//            if (type == typeof(bool))
//                return Value;

//            return Maybe<object>.None;
//        }

//        /// <inheritdoc />
//        public override string ToString() => Serialize();
//    } //TODO constant values

//    /// <summary>
//    /// An enumeration value
//    /// </summary>
//    public record EnumerationValue(ISCLEnum Value) : ISCLObject(Value)
//    {
//        /// <inheritdoc />
//        public override string Serialize() => Value.ToString();

//        /// <inheritdoc />
//        public override string Serialize() => Value.ToString();

//        /// <inheritdoc />
//        public override string GetFormattedString(
//            char delimiter,
//            string dateTimeFormat) => Value.ToString();

//        /// <inheritdoc />
//        protected override Maybe<object> AsType(Type type)
//        {
//            return Maybe<object>.None;
//        }

//        /// <inheritdoc />
//        public override SchemaNode ToSchemaNode(
//            string path,
//            SchemaConversionOptions? schemaConversionOptions)
//        {
//            return StringNode.Default;
//        }

//        /// <inheritdoc />
//        public override string ToString() => Serialize();
//    }

//    /// <summary>
//    /// A date time value
//    /// </summary>
//    public record DateTime(System.DateTime Value) : ISCLObject(Value)
//    {
//        /// <inheritdoc />
//        public override SchemaNode ToSchemaNode(
//            string path,
//            SchemaConversionOptions? schemaConversionOptions)
//        {
//            return new StringNode(
//                EnumeratedValuesNodeData.Empty,
//                DateTimeStringFormat.Instance,
//                StringRestrictions.NoRestrictions
//            );
//        }

//        /// <inheritdoc />
//        public override string Serialize() => Value.ToString(Constants.DateTimeFormat);

//        /// <inheritdoc />
//        public override string Serialize() => Value.ToString(Constants.DateTimeFormat);

//        /// <inheritdoc />
//        public override string GetFormattedString(
//            char delimiter,
//            string dateTimeFormat) => Value.ToString(dateTimeFormat);

//        /// <inheritdoc />
//        protected override Maybe<object> AsType(Type type)
//        {
//            if (type == typeof(System.DateTime))
//                return Value;

//            return Maybe<object>.None;
//        }

//        /// <inheritdoc />
//        public override string ToString() => Serialize();
//    }

//    /// <summary>
//    /// A nested entity value
//    /// </summary>
//    public record NestedEntity(Entity Value) : ISCLObject(Value)
//    {
//        /// <inheritdoc />
//        public override string Serialize() => Value.ToString();

//        /// <inheritdoc />
//        public override string Serialize() => Value.Serialize();

//        /// <inheritdoc />
//        public override string GetFormattedString(
//            char delimiter,
//            string dateTimeFormat) => Value.ToString();

//        /// <inheritdoc />
//        protected override Maybe<object> AsType(Type type)
//        {
//            if (type == typeof(Entity))
//                return Value;

//            return Maybe<object>.None;
//        }

//        /// <inheritdoc />
//        public override string ToString() => Value.ToString();

//        /// <inheritdoc />
//        public override SchemaNode ToSchemaNode(
//            string path,
//            SchemaConversionOptions? schemaConversionOptions)
//        {
//            var dictionary = new Dictionary<string, (SchemaNode Node, bool Required)>();

//            foreach (var (key, property) in Value.Dictionary.OrderBy(x => x.Value.Order))
//            {
//                var node = property.Value.ToSchemaNode($"{path}/{key}", schemaConversionOptions);
//                dictionary[key] = (node, true);
//            }

//            return new EntityNode(
//                EnumeratedValuesNodeData.Empty,
//                new EntityAdditionalItems(FalseNode.Instance),
//                new EntityPropertiesData(dictionary)
//            );
//        }
//    }

//    /// <summary>
//    /// A list of values
//    /// </summary>
//    public record NestedList(ImmutableList<ISCLObject> Value) : ISCLObject(Value)
//    {
//        /// <inheritdoc />
//        public override string Serialize() =>
//            SerializationMethods.SerializeList(Value.Select(y => y.Serialize()));

//        /// <inheritdoc />
//        public override string Serialize()
//        {
//            return SerializationMethods.SerializeList(Value.Select(y => y.Serialize()));
//        }

//        /// <inheritdoc />
//        public override string GetFormattedString(
//            char delimiter,
//            string dateTimeFormat) => string.Join(
//            delimiter,
//            Value.Select(ev1 => ev1.GetFormattedString(delimiter, dateTimeFormat))
//        );

//        /// <inheritdoc />
//        public virtual bool Equals(NestedList? other)
//        {
//            return other is not null && Value.SequenceEqual(other.Value);
//        }

//        /// <inheritdoc />
//        public override int GetHashCode()
//        {
//            if (Value.IsEmpty)
//                return 0;

//            return HashCode.Combine(Value.First(), Value.Last(), Value.Count);
//        }

//        /// <inheritdoc />
//        public override string ToString()
//        {
//            return Value.Count + " elements";
//        }

//        /// <inheritdoc />
//        public override SchemaNode ToSchemaNode(
//            string path,
//            SchemaConversionOptions? schemaConversionOptions)
//        {
//            SchemaNode additionalItems = new TrueNode();

//            for (var index = 0; index < Value.Count; index++)
//            {
//                var entityValue = Value[index];
//                var n = entityValue.ToSchemaNode(path + $"[{index}]", schemaConversionOptions);
//                additionalItems = additionalItems.Combine(n);
//            }

//            return new ArrayNode(
//                EnumeratedValuesNodeData.Empty,
//                new ItemsData(ImmutableList<SchemaNode>.Empty, additionalItems)
//            );
//        }

//        /// <inheritdoc />
//        protected override Maybe<object> AsType(Type type)
//        {
//            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Array<>))
//            {
//                var genericType = type.GenericTypeArguments[0];

//                var elements = Value.Select(x => x.TryGetValue(genericType))
//                    .Combine(ErrorBuilderList.Combine);

//                if (elements.IsFailure)
//                    return elements.ConvertFailure<object>();

//                var createArrayMethod =
//                    typeof(ArrayHelper).GetMethod(nameof(ArrayHelper.CreateArray))!
//                        .MakeGenericMethod(genericType);

//                var arrayInstance = createArrayMethod.Invoke(
//                    null,
//                    new object?[] { elements.Value }
//                );

//                return arrayInstance!;
//            }

//            return Maybe<object>.None;
//        }
//    }

//    /// <summary>
//    /// Create an entity from structured entity properties
//    /// </summary>
//    public static ISCLObject CreateFromProperties(
//        IReadOnlyList<(Maybe<EntityPropertyKey> key, object? argValue)> properties)
//    {
//        if (properties.Count == 0)
//            return Null.Instance;

//        if (properties.Count == 1 && properties.Single().key.HasNoValue)
//            return CreateFromObject(properties.Single().argValue);

//        var entityProperties =
//            new Dictionary<string, EntityProperty>(StringComparer.OrdinalIgnoreCase);

//        void SetEntityProperty(string key, ISCLObject ev)
//        {
//            EntityProperty newProperty;

//            if (entityProperties.TryGetValue(key, out var existingValue))
//            {
//                if (ev is NestedEntity nestedEntity)
//                {
//                    if (existingValue.Value is NestedEntity existingNestedEntity)
//                    {
//                        var nEntity = existingNestedEntity.Value.Combine(nestedEntity.Value);

//                        newProperty = new EntityProperty(
//                            key,
//                            new NestedEntity(nEntity),
//                            existingValue.Order
//                        );
//                    }
//                    else
//                    {
//                        //Ignore the old property
//                        newProperty = new EntityProperty(key, ev, existingValue.Order);
//                    }
//                }
//                else if (existingValue.Value is NestedEntity existingNestedEntity)
//                {
//                    var nEntity =
//                        existingNestedEntity.Value.WithProperty(Entity.PrimitiveKey, ev, null);

//                    newProperty = new EntityProperty(
//                        key,
//                        new NestedEntity(nEntity),
//                        existingValue.Order
//                    );
//                }
//                else //overwrite the existing property
//                    newProperty = new EntityProperty(key, ev, existingValue.Order);
//            }
//            else //New property
//                newProperty = new EntityProperty(key, ev, entityProperties.Count);

//            entityProperties[key] = newProperty;
//        }

//        foreach (var (key, argValue) in properties)
//        {
//            if (key.HasNoValue)
//            {
//                var ev = CreateFromObject(argValue);

//                if (ev is NestedEntity ne)
//                    foreach (var (nestedKey, value) in ne.Value.Dictionary)
//                        SetEntityProperty(nestedKey, value.Value);
//                else
//                    SetEntityProperty(Entity.PrimitiveKey, ev);
//            }
//            else
//            {
//                var (firstKey, remainder) = key.GetValueOrThrow().Split();

//                var ev = CreateFromProperties(new[] { (remainder, argValue) });

//                SetEntityProperty(firstKey, ev);
//            }
//        }

//        var newEntity = new Entity(entityProperties.ToImmutableDictionary());

//        return new NestedEntity(newEntity);
//    }

//    /// <summary>
//    /// Create an entity from an object
//    /// </summary>
//    public static ISCLObject CreateFromObject(object? argValue)
//    {
//        switch (argValue)
//        {
//            case null:             return Null.Instance;
//            case SCLNull:          return Null.Instance;
//            case ISCLObject ev:    return ev;
//            case StringStream ss1: return CreateFromString(ss1.GetString());
//            case string s:         return CreateFromString(s);
//            case int i:            return new Integer(i);
//            case byte @byte:       return new Integer(@byte);
//            case short @short:     return new Integer(@short);
//            case bool b:           return new Boolean(b);
//            case double d:         return new Double(d);
//            case long ln and < int.MaxValue and > int.MinValue:
//                return new Integer(Convert.ToInt32(ln));
//            case ISCLEnum e1:           return new EnumerationValue(e1);
//            case System.DateTime dt:    return new DateTime(dt);
//            case Enum e:                return new EnumerationValue(e.ConvertToSCLEnum());
//            case JsonElement je:        return Create(je);
//            case Entity entity:         return new NestedEntity(entity);
//            case IEntityConvertible ec: return new NestedEntity(ec.ConvertToEntity());
//            case Version version:       return new String(version.ToString());

//            case IDictionary dict:
//            {
//                var builder = ImmutableDictionary<string, EntityProperty>.Empty.ToBuilder();
//                builder.KeyComparer = StringComparer.OrdinalIgnoreCase;
//                var i = 0;

//                foreach (DictionaryEntry dictionaryEntry in dict)
//                {
//                    var val = dictionaryEntry.Value;
//                    var ev  = CreateFromObject(val);
//                    var ep  = new EntityProperty(dictionaryEntry.Key.ToString()!, ev, i);
//                    builder.Add(dictionaryEntry.Key.ToString()!, ep);
//                    i++;
//                }

//                var entity = new Entity(builder.ToImmutable());
//                return new NestedEntity(entity);
//            }
//            case IEnumerable e2:
//            {
//                var newEnumerable = e2.Cast<object>()
//                    .Select(CreateFromObject)
//                    .ToImmutableList();

//                if (!newEnumerable.Any())
//                    return Null.Instance;

//                return new NestedList(newEnumerable);
//            }
//            case IResult:
//                throw new ArgumentException(
//                    "Attempt to set ISCLObject to a Result - you should check the result for failure and then set it to the value of the result",
//                    nameof(argValue)
//                );

//            default:

//            {
//                if (argValue.GetType()
//                        .GetCustomAttributes(true)
//                        .OfType<SerializableAttribute>()
//                        .Any() ||
//                    argValue.GetType()
//                        .GetCustomAttributes(true)
//                        .OfType<DataContractAttribute>()
//                        .Any()
//                   )
//                {
//                    var entity = EntityConversionHelpers.ConvertToEntity(argValue);
//                    return new NestedEntity(entity);
//                }

//                throw new ArgumentException(
//                    $"Attempt to set ISCLObject to {argValue.GetType().Name}"
//                );
//            }
//        }

//        static ISCLObject CreateFromString(string s)
//        {
//            return new String(s);
//        }
//    }

//    /// <summary>
//    /// Create an ISCLObject from a JsonElement
//    /// </summary>
//    public static ISCLObject Create(JsonElement element)
//    {
//        switch (element.ValueKind)
//        {
//            case JsonValueKind.Undefined: return Null.Instance;
//            case JsonValueKind.Object:
//            {
//                var dict = element.EnumerateObject()
//                    .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
//                    .Select(
//                        (x, i) =>
//                            new EntityProperty(x.First().Name, Create(x.First().Value), i)
//                    )
//                    .ToImmutableDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

//                var entity = new Entity(dict);
//                return new NestedEntity(entity);
//            }
//            case JsonValueKind.Array:
//            {
//                var list = element.EnumerateArray().Select(Create).ToImmutableList();
//                return new NestedList(list);
//            }
//            case JsonValueKind.String: return new String(element.GetString()!);
//            case JsonValueKind.Number:
//            {
//                if (element.TryGetInt32(out var i))
//                    return new Integer(i);

//                return new Double(element.GetDouble());
//            }
//            case JsonValueKind.True: return new Boolean(true);
//            case JsonValueKind.False: return new Boolean(false);
//            case JsonValueKind.Null: return Null.Instance;
//            default: throw new ArgumentOutOfRangeException(element.ValueKind.ToString());
//        }
//    }

//    /// <summary>
//    /// Gets a schema node which could match this entity value
//    /// </summary>
//    [Pure]
//    public abstract SchemaNode ToSchemaNode(
//        string path,
//        SchemaConversionOptions? schemaConversionOptions);

//    /// <summary>
//    /// Convert this Entity to a Json Element
//    /// </summary>
//    [Pure]
//    public JsonElement ToJsonElement()
//    {
//        var options = new JsonSerializerOptions()
//        {
//            Converters = { new JsonStringEnumConverter() }
//        };

//        var element = ObjectValue.ToJsonDocument(options).RootElement.Clone();

//        return element;
//    }

//    /// <summary>
//    /// If this is a primitive, get a string representation
//    /// </summary>
//    public abstract string Serialize();

//    /// <summary>
//    /// Serialize this value as it will appear in SCL
//    /// </summary>
//    public abstract string Serialize();

//    /// <summary>
//    /// Gets a string with the given format
//    /// </summary>
//    public abstract string GetFormattedString(
//        char delimiter,
//        string dateTimeFormat);

//    /// <summary>
//    /// Gets the default entity value for a particular type
//    /// </summary>
//    public static T GetDefaultValue<T>()
//    {
//        if (Entity.Empty is T tEntity)
//            return tEntity;

//        if (StringStream.Empty is T tStringStream)
//            return tStringStream;

//        if ("" is T tString)
//            return tString;

//        if (typeof(T).IsAssignableTo(typeof(IArray)) && typeof(T).IsGenericType)
//        {
//            var param = typeof(T).GenericTypeArguments[0];
//            var array = typeof(EagerArray<>).MakeGenericType(param);

//            var arrayInstance = Activator.CreateInstance(array);

//            return (T)arrayInstance!;
//        }

//        return default!;
//    }

//    /// <summary>
//    /// Returns the value, if it is of a particular type
//    /// </summary>
//    protected abstract Maybe<object> AsType(Type type);

//    private Result<object, IErrorBuilder> TryGetValue(Type type)
//    {
//        var maybeObject = AsType(type);

//        if (maybeObject.HasValue)
//            return maybeObject.GetValueOrThrow();

//        var primitive = Serialize();

//        if (type == typeof(int))
//        {
//            if (int.TryParse(primitive, out var i))
//                return i;
//        }

//        else if (type == typeof(double))
//        {
//            if (double.TryParse(primitive, out var d))
//                return d;
//        }
//        else if (type == typeof(bool))
//        {
//            if (bool.TryParse(primitive, out var b))
//                return b;
//        }
//        else if (type.IsEnum)
//        {
//            if (this is EnumerationValue enumeration)
//            {
//                if (Enum.TryParse(type, enumeration.Value.EnumValue, true, out var ro))
//                    return ro!;
//            }
//            else if (!int.TryParse(primitive, out _) && //prevent int conversion
//                     Enum.TryParse(type, primitive, true, out var r)
//                    )
//                return r!;
//        }
//        else if (type == typeof(System.DateTime))
//        {
//            if (!double.TryParse(primitive, out _) && //prevent double conversion
//                System.DateTime.TryParse(primitive, out var d))
//                return d;
//        }
//        else if (type == typeof(string))
//        {
//            var ser = Serialize();

//            return ser;
//        }
//        else if (type == typeof(StringStream))
//        {
//            var ser = new StringStream(Serialize());

//            return ser;
//        }
//        else if (type == typeof(object))
//        {
//            return AsISCLObject(ObjectValue); // new StringStream(Serialize());
//        }
//        else if (type.GetInterfaces().Contains(typeof(IOneOf)))
//        {
//            var i = 0;

//            foreach (var typeGenericTypeArgument in type.GenericTypeArguments)
//            {
//                var value = TryGetValue(typeGenericTypeArgument);

//                if (value.IsSuccess)
//                {
//                    var methodName = $"FromT{i}";

//                    var method = type.GetMethod(
//                        methodName,
//                        BindingFlags.Static | BindingFlags.Public
//                    )!;

//                    var oneOfThing = method.Invoke(null, new[] { value.Value })!;

//                    return Result.Success<object, IErrorBuilder>(oneOfThing);
//                }

//                i++;
//            }
//        }

//        return ErrorCode.CouldNotConvertISCLObject.ToErrorBuilder(type.Name);
//    }

//    private static object AsISCLObject(object? o)
//    {
//        if (o is null)
//            return SCLNull.Instance;

//        if (o is int i)
//            return i;

//        if (o is double d)
//            return d;

//        if (o is string s)
//            return new StringStream(s);

//        if (o is StringStream ss)
//            return ss;

//        if (o is System.DateTime dt)
//            return dt;

//        if (o is IArray)
//            return o;

//        if (o is Entity e)
//            return e;

//        if (o is IEnumerable enumerable)
//            return SerializationMethods.SerializeList(
//                enumerable.OfType<ISCLObject>().Select(x => x.Serialize())
//            );

//        if (o is ISCLEnum enumeration)
//            return enumeration;

//        return SCLNull.Instance;
//    }

//    /// <summary>
//    /// Tries to get the value if it is a particular type
//    /// </summary>
//    public Result<T, IErrorBuilder> TryGetValue<T>()
//    {
//        var r = TryGetValue(typeof(T));

//        if (r.IsFailure)
//            return r.ConvertFailure<T>();

//        return (T)r.Value!;
//    }
//}


