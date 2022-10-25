using System.IO;
using System.Text.Json.Nodes;
using System.Text;
using System.Text.Json.Serialization;
using Generator.Equals;

// ReSharper disable once CheckNamespace
namespace Reductech.Sequence.Core;

/// <summary>
/// An SCL entity object
/// </summary>
[JsonConverter(typeof(EntityJsonConverter))]
[Equatable]
public partial record struct Entity(
    [property: OrderedEquality] ImmutableArray<EntityKey> Headers,
    [property: OrderedEquality] ImmutableArray<ISCLObject> Values);

public partial record struct Entity : ISCLObject, IEnumerable<KeyValuePair<EntityKey, ISCLObject>>
{
    /// <summary>
    /// Empty EntityStruct
    /// </summary>
    public static readonly Entity Empty = new(
        ImmutableArray<EntityKey>.Empty,
        ImmutableArray<ISCLObject>.Empty
    );

    /// <inheritdoc />
    public TypeReference GetTypeReference() => TypeReference.Entity.NoSchema;

    private static readonly ImmutableArray<EntityKey> PrimitiveKeyHeaders =
        ImmutableArray.Create(EntityKey.Primitive);

    /// <summary>
    /// Create an entity with a single primitive value
    /// </summary>
    public static Entity CreatePrimitive(ISCLObject value)
    {
        return new Entity(PrimitiveKeyHeaders, ImmutableArray.Create(value));
    }

    /// <summary>
    /// Creates a new EntityStruct. This will convert CSharp arguments to SCLObjects
    /// </summary>
    [Pure]
    public static Entity Create(params (string key, object? value)[] properties) => Create(
        properties.Select(
            x => (new EntityNestedKey(x.key), ISCLObject.CreateFromCSharpObject(x.value))
        )
    );

    /// <summary>
    /// Create an EntityStruct from a CSharp dictionary
    /// </summary>
    [Pure]
    public static Entity Create(IDictionary dictionary)
    {
        var keys   = new EntityKey[dictionary.Count];
        var values = new ISCLObject[dictionary.Count];
        //List<EntityProperty> properties = new();
        var enumerator = dictionary.GetEnumerator();

        var i = 0;

        while (enumerator.MoveNext())
        {
            var k       = enumerator.Key.ToString()!;
            var vObject = ISCLObject.CreateFromCSharpObject(enumerator.Value);
            keys[i]   = new EntityKey(k);
            values[i] = vObject;
            i++;
        }

        return new Entity(keys.ToImmutableArray(), values.ToImmutableArray());
    }

    /// <summary>
    /// Create an EntityStruct from a JsonElement
    /// </summary>
    [Pure]
    public static Entity Create(JsonElement element)
    {
        var ev = element.ConvertToSCLObject();

        if (ev is Entity nestedEntityStruct)
            return nestedEntityStruct;

        return new Entity(PrimitiveKeyHeaders, ImmutableArray.Create(ev));
    }

    /// <summary>
    /// Create an EntityStruct from a JsonNode
    /// </summary>
    [Pure]
    public static Entity Create(JsonNode node)
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        var rootElement = node.ToJsonDocument()?.RootElement;
        return rootElement.HasValue ? Create(rootElement.Value) : Empty;
    }

    /// <summary>
    /// Convert this EntityStruct to a Json Element
    /// </summary>
    [Pure]
    public JsonElement ToJsonElement()
    {
        var stream = new MemoryStream();
        var writer = new Utf8JsonWriter(stream);

        EntityJsonConverter.Instance.Write(
            writer,
            this,
            ISCLObject.DefaultJsonSerializerOptions
        );

        var reader = new Utf8JsonReader(stream.ToArray());

        using var document = JsonDocument.ParseValue(ref reader);
        return document.RootElement.Clone();
    }

    /// <summary>
    /// Creates a new EntityStruct
    /// </summary>
    [Pure]
    public static Entity Create(IEnumerable<(EntityNestedKey key, ISCLObject value)> properties)
    {
        var keys   = ImmutableArray.CreateBuilder<EntityKey>();
        var values = ImmutableArray.CreateBuilder<ISCLObject>();

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
                    group =>
                    {
                        var value = CreateFromProperties(group.ToList());

                        return (group.Key, value);
                    }
                );

        foreach (var (key, value) in allProperties)
        {
            keys.Add(key);
            values.Add(value);
        }

        return new Entity(keys.ToImmutable(), values.ToImmutable());
    }

    /// <summary>
    /// Creates a copy of this with the property added. Does not check if a property with that name already exists
    /// </summary>
    [Pure]
    public Entity WithPropertyAdded(EntityKey key, ISCLObject newValue, int? index)
    {
        ImmutableArray<EntityKey>  newHeaders;
        ImmutableArray<ISCLObject> newValues;

        if (index.HasValue)
        {
            newHeaders = Headers.Insert(index.Value, key);
            newValues  = Values.Insert(index.Value, newValue);
        }
        else
        {
            newHeaders = Headers.Add(key);
            newValues  = Values.Add(newValue);
        }

        return new Entity(newHeaders, newValues);
    }

    /// <summary>
    /// Change the index of a property. Returns none if the property was not present of had the same index.
    /// </summary>
    public Maybe<Entity> WithPropertyMoved(EntityKey key, int newIndex)
    {
        var oldIndex = Headers.IndexOf(key);

        if (oldIndex < 0 || oldIndex == newIndex)
        {
            return Maybe<Entity>.None;
        }

        var value = Values[oldIndex];

        var newHeaders = Headers.RemoveAt(oldIndex);
        newIndex   = Math.Min(newHeaders.Length, newIndex); //prevent exception
        newHeaders = newHeaders.Insert(newIndex, key);

        var newValues = Values.RemoveAt(oldIndex).Insert(newIndex, value);

        return new Entity(newHeaders, newValues);
    }

    /// <summary>
    /// Creates a copy of this with the property added or updated
    /// </summary>
    [Pure]
    public Entity WithPropertyAddedOrUpdated(EntityKey key, ISCLObject newValue)
    {
        ImmutableArray<EntityKey>  newHeaders;
        ImmutableArray<ISCLObject> newValues;

        var index = Headers.IndexOf(key);

        if (index < 0) //Add as a new property
        {
            newHeaders = Headers.Add(key);
            newValues  = Values.Add(newValue);
        }
        else
        {
            newHeaders = Headers;
            newValues  = Values.SetItem(index, newValue);
        }

        return new Entity(newHeaders, newValues);
    }

    /// <summary>
    /// Rename a property of this entity
    /// </summary>
    [Pure]
    public Maybe<Entity> WithPropertyRenamed(EntityKey oldKey, EntityKey newKey)
    {
        var index = Headers.IndexOf(oldKey);

        if (index < 0)
            return Maybe<Entity>.None; //No property to remove

        var newHeaders = Headers.SetItem(index, newKey);
        return this with { Headers = newHeaders };
    }

    /// <summary>
    /// Returns a copy of this EntityStruct with the specified property removed
    /// </summary>
    [Pure]
    public Entity WithPropertyRemoved(EntityKey key)
    {
        var index = Headers.IndexOf(key);

        if (index < 0)
            return this; //No property to remove

        var newHeaders = Headers.RemoveAt(index);
        var newValues  = Values.RemoveAt(index);

        return new Entity(newHeaders, newValues);
    }

    /// <summary>
    /// Try to remove a property from the EntityStruct
    /// Removing the last nested property on an EntityStruct will also remove that EntityStruct
    /// </summary>
    public Maybe<Entity> WithNestedPropertyRemoved(EntityNestedKey entityNestedKey)
    {
        var (firstKey, remainder) = entityNestedKey.Split();

        var index = Headers.IndexOf(firstKey);

        if (index < 0)
            return Maybe<Entity>.None;

        if (remainder.HasNoValue)
        {
            var newHeaders = Headers.RemoveAt(index);
            var newValues  = Values.RemoveAt(index);

            return new Entity(newHeaders, newValues);
        }

        var value = Values[index];

        if (value is not Entity nestedEntityStruct)
            return Maybe<Entity>.None;

        {
            var rem = remainder.GetValueOrThrow();
            var em  = nestedEntityStruct.WithNestedPropertyRemoved(rem);

            if (!em.TryGetValue(out var newNestedEntityStruct))
                return Maybe<Entity>.None;

            if (newNestedEntityStruct.IsEmpty())
            {
                var newHeaders = Headers.RemoveAt(index);
                var newValues  = Values.RemoveAt(index);

                return new Entity(newHeaders, newValues);
            }
            else
            {
                var newValues = Values.SetItem(index, newNestedEntityStruct);
                return this with { Values = newValues };
            }
        }
    }

    /// <summary>
    /// Combine two entities.
    /// Properties on the other EntityStruct take precedence.
    /// </summary>
    [Pure]
    public Entity Combine(Entity other)
    {
        var current = this;

        foreach (var ep in other)
            current = current.WithPropertyAddedOrUpdated(ep.Key, ep.Value);

        return current;
    }

    /// <summary>
    /// Try to get the value of a particular property
    /// </summary>
    [Pure]
    public Maybe<ISCLObject> TryGetValue(string key) => TryGetValue(new EntityNestedKey(key));

    /// <summary>
    /// Try to get the value of a particular property
    /// </summary>
    [Pure]
    public Maybe<ISCLObject> TryGetValue(EntityNestedKey key) =>
        TryGetProperty(key).Map(x => x.Value);

    /// <summary>
    /// Try to get a particular property
    /// </summary>
    [Pure]
    public Maybe<KeyValuePair<EntityKey, ISCLObject>> TryGetProperty(EntityNestedKey key)
    {
        var (firstKey, remainder) = key.Split();

        var index = Headers.IndexOf(firstKey);

        if (index < 0)
            return Maybe<KeyValuePair<EntityKey, ISCLObject>>.None;

        var value = Values[index];

        if (remainder.HasNoValue)
            return new KeyValuePair<EntityKey, ISCLObject>(firstKey, value);

        if (value is Entity nestedEntityStruct)
            return nestedEntityStruct.TryGetProperty(remainder.GetValueOrThrow());
        //We can't get the nested property as this is not an EntityStruct

        return
            Maybe<KeyValuePair<EntityKey, ISCLObject>>.None;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<KeyValuePair<EntityKey, ISCLObject>> GetEnumerator() => Headers.Zip(Values)
        .Select(tuple => new KeyValuePair<EntityKey, ISCLObject>(tuple.First, tuple.Second))
        .GetEnumerator();

    /// <inheritdoc />
    public string Serialize(SerializeOptions options)
    {
        var sb = new StringBuilder();

        sb.Append('(');

        var results = new List<string>();

        foreach (var (key, value) in this)
            results.Add($"'{key}': {value.Serialize(options)}");

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

        foreach (var (key, sclObject) in this)
        {
            var value = sclObject.ToCSharpObject();
            dictionary.Add(key.Inner, value);
        }

        return dictionary;
    }

    /// <summary>
    /// Format this EntityStruct with the default formatting options
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
        if (Headers.Length <= 1)
        {
            indentationStringBuilder.Append("(");

            //TODO use compound names for nested entities

            foreach (var (name, sclObject) in this)
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

            var longestPropertyName = Headers.Select(x => x.Inner.Length).Max();

            indentationStringBuilder.AppendJoin(
                "",
                true,
                this,
                ep =>
                {
                    indentationStringBuilder.Append(
                        $"{$"'{ep.Key.Inner}'".PadRight(longestPropertyName + 2)}: "
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
    /// Create an EntityStruct from structured EntityStruct properties
    /// </summary>
    [Pure]
    private static ISCLObject CreateFromProperties(
        IReadOnlyList<(Maybe<EntityNestedKey> key, ISCLObject argValue)> properties)
    {
        if (properties.Count == 0)
            return SCLNull.Instance;

        if (properties.Count == 1 && properties.Single().key.HasNoValue)
            return properties.Single().argValue;

        var entityStructProperties =
            new Dictionary<EntityKey, ISCLObject>();

        void SetEntityProperty(EntityKey key, ISCLObject ev)
        {
            ISCLObject newValue;

            if (entityStructProperties.TryGetValue(key, out var existingValue))
            {
                if (ev is Entity nestedEntityStruct)
                {
                    if (existingValue is Entity existingNestedEntityStruct)
                    {
                        newValue = existingNestedEntityStruct.Combine(nestedEntityStruct);
                    }
                    else
                    {
                        //Ignore the old property
                        newValue = ev;
                    }
                }
                else if (existingValue is Entity existingNestedEntityStruct)
                {
                    newValue =
                        existingNestedEntityStruct.WithPropertyAddedOrUpdated(
                            EntityKey.Primitive,
                            ev
                        );
                }
                else //overwrite the existing property
                    newValue = ev;
            }
            else //New property
                newValue = ev;

            entityStructProperties[key] = newValue;
        }

        foreach (var (key, argValue) in properties)
        {
            if (key.HasNoValue)
            {
                if (argValue is Entity ne)
                    foreach (var ep in ne)
                        SetEntityProperty(ep.Key, ep.Value);
                else
                    SetEntityProperty(EntityKey.Primitive, argValue);
            }
            else
            {
                var (firstKey, remainder) = key.GetValueOrThrow().Split();

                var ev = CreateFromProperties(new[] { (remainder, argValue) });

                SetEntityProperty(firstKey, ev);
            }
        }

        var newEntityStruct = new Entity(
            entityStructProperties.Keys.ToImmutableArray(),
            entityStructProperties.Values.ToImmutableArray()
        );

        return newEntityStruct;
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

        foreach (var ep in this)
        {
            var node = ep.Value.ToSchemaNode($"{path}/{ep.Key.Inner}", schemaConversionOptions);
            dictionary[ep.Key.Inner] = (node, true, order);
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
    public bool IsEmpty() => Headers.IsEmpty;
}
