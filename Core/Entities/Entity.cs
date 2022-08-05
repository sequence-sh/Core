using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Generator.Equals;

// ReSharper disable once CheckNamespace - we want this namespace to prevent clash with FunctionalExtensions
namespace Reductech.Sequence.Core;

/// <summary>
/// A piece of data.
/// </summary>
[JsonConverter(typeof(EntityJsonConverter))]
[Equatable]
public sealed partial record Entity(
    [property: OrderedEquality] ImmutableDictionary<string, EntityProperty> Dictionary) :
    ISCLObject, IEnumerable<EntityProperty>
{
    /// <summary>
    /// Creates a new entity from a dictionary.
    /// Be sure to set the string comparer on the dictionary.
    /// </summary>
    /// <summary>
    /// Creates a new entity
    /// </summary>
    public Entity(IEnumerable<EntityProperty> properties) : this(
        properties.GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToImmutableDictionary(
                x => x.Key,
                x => x.First(),
                StringComparer.OrdinalIgnoreCase
            )
    ) { }

    /// <summary>
    /// Empty entity
    /// </summary>
    public static Entity Empty { get; } = new(new List<EntityProperty>());

    /// <inheritdoc />
    public TypeReference GetTypeReference() => TypeReference.Entity.NoSchema;

    /// <summary>
    /// The default property name if the Entity represents a single primitive.
    /// </summary>
    public const string PrimitiveKey = "value";

    /// <summary>
    /// Creates a new Entity. This will convert CSharp arguments to SCLObjects
    /// </summary>
    [Pure]
    public static Entity Create(params (string key, object? value)[] properties) => Create(
        properties.Select(
            x => (new EntityPropertyKey(x.key), ISCLObject.CreateFromCSharpObject(x.value))
        )
    );

    /// <summary>
    /// Create an entity from a CSharp dictionary
    /// </summary>
    [Pure]
    public static Entity Create(IDictionary dictionary)
    {
        List<EntityProperty> properties = new();
        var                  enumerator = dictionary.GetEnumerator();

        var i = 0;

        while (enumerator.MoveNext())
        {
            var k       = enumerator.Key.ToString()!;
            var vObject = ISCLObject.CreateFromCSharpObject(enumerator.Value);
            properties.Add(new EntityProperty(k, vObject, i));
            i++;
        }

        return new Entity(properties);
    }

    /// <summary>
    /// Create an entity from a JsonElement
    /// </summary>
    [Pure]
    public static Entity Create(JsonElement element)
    {
        var ev = element.ConvertToSCLObject();

        if (ev is Entity nestedEntity)
            return nestedEntity;

        return new Entity(new[] { new EntityProperty(PrimitiveKey, ev, 0) });
    }

    /// <summary>
    /// Create an entity from a JsonNode
    /// </summary>
    [Pure]
    public static Entity Create(JsonNode node)
    {
        var rootElement = node.ToJsonDocument()?.RootElement;

        if (rootElement.HasValue)
        {
            return Create(rootElement.Value);
        }
        else
        {
            return Empty;
        }
    }

    /// <summary>
    /// Convert this Entity to a Json Element
    /// </summary>
    [Pure]
    public JsonElement ToJsonElement()
    {
        var stream = new MemoryStream();
        var writer = new Utf8JsonWriter(stream);

        EntityJsonConverter.Instance.Write(writer, this, ISCLObject.DefaultJsonSerializerOptions);

        var reader = new Utf8JsonReader(stream.ToArray());

        using var document = JsonDocument.ParseValue(ref reader);
        return document.RootElement.Clone();
    }

    /// <summary>
    /// Creates a new Entity
    /// </summary>
    [Pure]
    public static Entity Create(IEnumerable<(EntityPropertyKey key, ISCLObject value)> properties)
    {
        var allProperties =
            properties.Select(
                    p =>
                    {
                        var (firstKey, remainder) = p.key.Split();
                        return (firstKey, remainder, p.value);
                    }
                )
                .GroupBy(x => x.firstKey, x => (x.remainder, x.value))
                .Select(
                    (group, i) =>
                    {
                        var ev = CreateFromProperties(group.ToList());

                        return new EntityProperty(group.Key, ev, i);
                    }
                );

        return new Entity(allProperties);
    }

    /// <summary>
    /// Creates a copy of this with the property added or updated
    /// </summary>
    [Pure]
    public Entity WithProperty(string key, ISCLObject newValue, int? order)
    {
        EntityProperty newProperty;

        if (Dictionary.TryGetValue(key, out var ep))
        {
            ISCLObject newValue2;

            if (ep.Value is Entity existingEntity)
            {
                Entity combinedEntity;

                if (newValue is Entity newEntity)
                    combinedEntity = existingEntity.Combine(newEntity);
                else
                    combinedEntity = existingEntity.WithProperty(
                        PrimitiveKey,
                        newValue,
                        null
                    );

                newValue2 = combinedEntity;
            }
            else
            {
                //overwrite the property
                newValue2 = newValue;
            }

            newProperty = new EntityProperty(ep.Name, newValue2, order ?? ep.Order);
        }
        else
        {
            newProperty = new EntityProperty(key, newValue, order ?? Dictionary.Count);
        }

        var newDict = Dictionary.SetItem(key, newProperty);

        return new Entity(newDict);
    }

    /// <summary>
    /// Returns a copy of this entity with the specified property removed
    /// </summary>
    [Pure]
    public Entity RemoveProperty(string propertyName)
    {
        if (!Dictionary.ContainsKey(propertyName))
            return this; //No property to remove

        var newDict = Dictionary.Remove(propertyName);

        return new Entity(newDict);
    }

    /// <summary>
    /// Remove properties from an entity.
    /// Removing the last nested property on an entity will also remove that entity
    /// </summary>
    [Pure]
    public Maybe<Entity> TryRemoveProperties(IEnumerable<EntityPropertyKey> properties)
    {
        var newEntity = this;
        var changed   = false;

        foreach (var entityPropertyKey in properties)
        {
            var e = newEntity.TryRemoveProperty(entityPropertyKey);

            if (e.HasValue)
            {
                newEntity = e.GetValueOrThrow();
                changed   = true;
            }
        }

        if (!changed)
            return Maybe<Entity>.None;

        return newEntity;
    }

    /// <summary>
    /// Try to remove a property from the entity
    /// Removing the last nested property on an entity will also remove that entity
    /// </summary>
    public Maybe<Entity> TryRemoveProperty(EntityPropertyKey entityPropertyKey)
    {
        var (firstKey, remainder) = entityPropertyKey.Split();

        if (!Dictionary.ContainsKey(firstKey))
            return Maybe<Entity>.None;

        if (remainder.HasNoValue)
        {
            var newDict = Dictionary.Remove(firstKey);
            return new Entity(newDict);
        }

        if (!Dictionary.TryGetValue(firstKey, out var ep)
         || ep.Value is not Entity nestedEntity)
            return Maybe<Entity>.None;

        {
            var rem = remainder.GetValueOrThrow();
            var em  = nestedEntity.TryRemoveProperty(rem);

            if (!em.HasValue)
                return Maybe<Entity>.None;

            var newNestedEntity = em.GetValueOrThrow();

            if (newNestedEntity.Dictionary.IsEmpty)
            {
                var newDict = Dictionary.Remove(firstKey);
                return new Entity(newDict);
            }
            else
            {
                var newProperty = new EntityProperty(
                    firstKey,
                    newNestedEntity,
                    ep.Order
                );

                var newDict = Dictionary.SetItem(firstKey, newProperty);
                return new Entity(newDict);
            }
        }
    }

    /// <summary>
    /// Combine two entities.
    /// Properties on the other entity take precedence.
    /// </summary>
    [Pure]
    public Entity Combine(Entity other)
    {
        var current = this;

        foreach (var ep in other)
            current = current.WithProperty(ep.Name, ep.Value, null);

        return current;
    }

    /// <summary>
    /// Try to get the value of a particular property
    /// </summary>
    [Pure]
    public Maybe<ISCLObject> TryGetValue(string key) => TryGetValue(new EntityPropertyKey(key));

    /// <summary>
    /// Try to get the value of a particular property
    /// </summary>
    [Pure]
    public Maybe<ISCLObject> TryGetValue(EntityPropertyKey key) =>
        TryGetProperty(key).Map(x => x.Value);

    /// <summary>
    /// Try to get a particular property
    /// </summary>
    [Pure]
    public Maybe<EntityProperty> TryGetProperty(EntityPropertyKey key)
    {
        var (firstKey, remainder) = key.Split();

        if (!Dictionary.TryGetValue(firstKey, out var ep))
            return Maybe<EntityProperty>.None;

        if (remainder.HasNoValue)
            return ep;

        if (ep.Value is Entity nestedEntity)
            return nestedEntity.TryGetProperty(remainder.GetValueOrThrow());
        //We can't get the nested property as this is not an entity

        return
            Maybe<EntityProperty>.None;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<EntityProperty> GetEnumerator() =>
        Dictionary.Values.OrderBy(x => x.Order).GetEnumerator();

    /// <inheritdoc />
    public string Serialize(SerializeOptions options)
    {
        var sb = new StringBuilder();

        sb.Append('(');

        var results = new List<string>();

        foreach (var (name, entityValue, _) in this)
            results.Add($"'{name}': {entityValue.Serialize(options)}");

        sb.AppendJoin(" ", results);

        sb.Append(')');

        var result = sb.ToString();

        return result;
    }

    /// <inheritdoc />
    public override string ToString() => Serialize(SerializeOptions.Name);

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public object ToCSharpObject()
    {
        var dictionary = new Dictionary<string, object?>();

        foreach (var (name, sclObject, _) in this)
        {
            var value = sclObject.ToCSharpObject();
            dictionary.Add(name, value);
        }

        return dictionary;
    }

    /// <summary>
    /// Format this entity with the default formatting options
    /// </summary>
    /// <returns></returns>
    public string Format()
    {
        var sb = new IndentationStringBuilder();

        (this as ISCLObject).Format(
            sb,
            new FormattingOptions(),
            new Stack<Comment>()
        );

        return sb.ToString();
    }

    void ISerializable.Format(
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        if (Dictionary.Count <= 1)
        {
            indentationStringBuilder.Append("(");

            //TODO use compound names for nested entities

            foreach (var (name, sclObject, _) in this)
            {
                indentationStringBuilder.Append($"{name}: ");

                sclObject.Format(
                    indentationStringBuilder,
                    options,
                    remainingComments
                );
            }

            indentationStringBuilder.Append(")");
        }
        else
        {
            indentationStringBuilder.AppendLine("(");
            indentationStringBuilder.Indent();

            //TODO use compound names for nested entities

            var longestPropertyName = Dictionary.Keys.Select(x => x.Length).Max();

            indentationStringBuilder.AppendJoin(
                "",
                true,
                this,
                ep =>
                {
                    indentationStringBuilder.Append(
                        $"{$"'{ep.Name}'".PadRight(longestPropertyName + 2)}: "
                    );

                    ep.Value.Format(
                        indentationStringBuilder,
                        options,
                        remainingComments
                    );
                }
            );

            indentationStringBuilder.AppendLineMaybe();
            indentationStringBuilder.UnIndent();
            indentationStringBuilder.Append(")");
        }
    }

    /// <summary>
    /// Create an entity from structured entity properties
    /// </summary>
    [Pure]
    private static ISCLObject CreateFromProperties(
        IReadOnlyList<(Maybe<EntityPropertyKey> key, ISCLObject argValue)> properties)
    {
        if (properties.Count == 0)
            return SCLNull.Instance;

        if (properties.Count == 1 && properties.Single().key.HasNoValue)
            return properties.Single().argValue;

        var entityProperties =
            new Dictionary<string, EntityProperty>(StringComparer.OrdinalIgnoreCase);

        void SetEntityProperty(string key, ISCLObject ev)
        {
            EntityProperty newProperty;

            if (entityProperties.TryGetValue(key, out var existingValue))
            {
                if (ev is Entity nestedEntity)
                {
                    if (existingValue.Value is Entity existingNestedEntity)
                    {
                        var nEntity = existingNestedEntity.Combine(nestedEntity);

                        newProperty = new EntityProperty(
                            key,
                            nEntity,
                            existingValue.Order
                        );
                    }
                    else
                    {
                        //Ignore the old property
                        newProperty = new EntityProperty(key, ev, existingValue.Order);
                    }
                }
                else if (existingValue.Value is Entity existingNestedEntity)
                {
                    var nEntity =
                        existingNestedEntity.WithProperty(Entity.PrimitiveKey, ev, null);

                    newProperty = new EntityProperty(
                        key,
                        nEntity,
                        existingValue.Order
                    );
                }
                else //overwrite the existing property
                    newProperty = new EntityProperty(key, ev, existingValue.Order);
            }
            else //New property
                newProperty = new EntityProperty(key, ev, entityProperties.Count);

            entityProperties[key] = newProperty;
        }

        foreach (var (key, argValue) in properties)
        {
            if (key.HasNoValue)
            {
                if (argValue is Entity ne)
                    foreach (var (nestedKey, value) in ne.Dictionary)
                        SetEntityProperty(nestedKey, value.Value);
                else
                    SetEntityProperty(PrimitiveKey, argValue);
            }
            else
            {
                var (firstKey, remainder) = key.GetValueOrThrow().Split();

                var ev = CreateFromProperties(new[] { (remainder, argValue) });

                SetEntityProperty(firstKey, ev);
            }
        }

        var newEntity = new Entity(entityProperties.ToImmutableDictionary());

        return newEntity;
    }

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject
    {
        if (this is T value)
            return value;

        return Maybe<T>.None;
    }

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(
        string path,
        SchemaConversionOptions? schemaConversionOptions)
    {
        var dictionary = new Dictionary<string, (SchemaNode Node, bool Required, int Order)>();

        var order = 0;

        foreach (var (key, property) in Dictionary.OrderBy(x => x.Value.Order))
        {
            var node = property.Value.ToSchemaNode($"{path}/{key}", schemaConversionOptions);
            dictionary[key] = (node, true, order);
            order++;
        }

        return new EntityNode(
            EnumeratedValuesNodeData.Empty,
            new EntityAdditionalItems(FalseNode.Instance),
            new EntityPropertiesData(dictionary)
        );
    }

    /// <inheritdoc />
    public IConstantFreezableStep ToConstantFreezableStep(TextLocation location) =>
        new SCLConstantFreezable<Entity>(this, location);

    /// <inheritdoc />
    public bool IsEmpty()
    {
        return Dictionary.IsEmpty;
    }
}
