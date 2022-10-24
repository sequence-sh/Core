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
public partial record struct Entity(
    [property: OrderedEquality] ImmutableArray<string> Headers,
    [property: OrderedEquality] ImmutableArray<ISCLObject> Values)
{
    public bool Equals(Entity? other)
    {
        return other.HasValue && Equals(other.Value);
    }

    /// <inheritdoc/>
    public bool Equals(Entity other)
    {
        return
            OrderedEqualityComparer<string>.Default.Equals(Headers, other.Headers)
         && OrderedEqualityComparer<ISCLObject>.Default.Equals(Values, other.Values)
            ;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(
            Headers,
            OrderedEqualityComparer<string>.Default
        );

        hashCode.Add(
            Values,
            OrderedEqualityComparer<ISCLObject>.Default
        );

        return hashCode.ToHashCode();
    }
}

public partial record struct Entity : ISCLObject, IEnumerable<EntityProperty>
{
    /// <summary>
    /// Empty EntityStruct
    /// </summary>
    public static readonly Entity Empty = new(
        ImmutableArray<string>.Empty,
        ImmutableArray<ISCLObject>.Empty
    );

    /// <inheritdoc />
    public TypeReference GetTypeReference() => TypeReference.Entity.NoSchema;

    /// <summary>
    /// The default property name if the EntityStruct represents a single primitive.
    /// </summary>
    public const string PrimitiveKey = "value";

    private static readonly ImmutableArray<string> PrimitiveKeyHeaders =
        ImmutableArray<string>.Empty.Add("value");

    /// <summary>
    /// Create an entity with a single primitive value
    /// </summary>
    public static Entity CreatePrimitive(ISCLObject value)
    {
        return new Entity(PrimitiveKeyHeaders, ImmutableArray<ISCLObject>.Empty.Add(value));
    }

    /// <summary>
    /// Creates a new EntityStruct. This will convert CSharp arguments to SCLObjects
    /// </summary>
    [Pure]
    public static Entity Create(params (string key, object? value)[] properties) => Create(
        properties.Select(
            x => (new EntityPropertyKey(x.key), ISCLObject.CreateFromCSharpObject(x.value))
        )
    );

    /// <summary>
    /// Create an EntityStruct from a CSharp dictionary
    /// </summary>
    [Pure]
    public static Entity Create(IDictionary dictionary)
    {
        var keys   = new string[dictionary.Count];
        var values = new ISCLObject[dictionary.Count];
        //List<EntityProperty> properties = new();
        var enumerator = dictionary.GetEnumerator();

        var i = 0;

        while (enumerator.MoveNext())
        {
            var k       = enumerator.Key.ToString()!;
            var vObject = ISCLObject.CreateFromCSharpObject(enumerator.Value);
            keys[i]   = k;
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

        return new Entity(PrimitiveKeyHeaders, new ImmutableArray<ISCLObject>() { ev });
    }

    /// <summary>
    /// Create an EntityStruct from a JsonNode
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
    public static Entity Create(IEnumerable<(EntityPropertyKey key, ISCLObject value)> properties)
    {
        var keys   = ImmutableArray.CreateBuilder<string>();
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
                    (group, i) =>
                    {
                        var ev = CreateFromProperties(group.ToList());

                        return new EntityProperty(group.Key, ev, i);
                    }
                );

        foreach (var ep in allProperties)
        {
            keys.Add(ep.Name);
            values.Add(ep.Value);
        }

        return new Entity(keys.ToImmutable(), values.ToImmutable());
    }

    /// <summary>
    /// Creates a copy of this with the property added or updated
    /// </summary>
    [Pure]
    public Entity WithProperty(string key, ISCLObject newValue)
    {
        ImmutableArray<string>     newHeaders;
        ImmutableArray<ISCLObject> newValues;
        var                        index = Headers.IndexOf(key, StringComparer.OrdinalIgnoreCase);

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
    /// Returns a copy of this EntityStruct with the specified property removed
    /// </summary>
    [Pure]
    public Entity RemoveProperty(string propertyName)
    {
        var index = this.Headers.IndexOf(propertyName, StringComparer.OrdinalIgnoreCase);

        if (index < 0)
            return this; //No property to remove

        var newHeaders = Headers.RemoveAt(index);
        var newValues  = Values.RemoveAt(index);

        return new Entity(newHeaders, newValues);
    }

    /// <summary>
    /// Remove properties from an EntityStruct.
    /// Removing the last nested property on an EntityStruct will also remove that EntityStruct
    /// </summary>
    [Pure]
    public Maybe<Entity> TryRemoveProperties(IEnumerable<EntityPropertyKey> properties)
    {
        var newEntityStruct = this;
        var changed         = false;

        foreach (var EntityPropertyKey in properties)
        {
            var e = newEntityStruct.TryRemoveProperty(EntityPropertyKey);

            if (e.HasValue)
            {
                newEntityStruct = e.GetValueOrThrow();
                changed         = true;
            }
        }

        if (!changed)
            return Maybe<Entity>.None;

        return newEntityStruct;
    }

    /// <summary>
    /// Try to remove a property from the EntityStruct
    /// Removing the last nested property on an EntityStruct will also remove that EntityStruct
    /// </summary>
    public Maybe<Entity> TryRemoveProperty(EntityPropertyKey EntityPropertyKey)
    {
        var (firstKey, remainder) = EntityPropertyKey.Split();

        var index = Headers.IndexOf(firstKey, StringComparer.OrdinalIgnoreCase);

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
            var em  = nestedEntityStruct.TryRemoveProperty(rem);

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
            current = current.WithProperty(ep.Name, ep.Value);

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

        var index = Headers.IndexOf(firstKey, StringComparer.OrdinalIgnoreCase);

        if (index < 0)
            return Maybe<EntityProperty>.None;

        var value = Values[index];

        if (remainder.HasNoValue)
            return new EntityProperty(firstKey, value, index);

        if (value is Entity nestedEntityStruct)
            return nestedEntityStruct.TryGetProperty(remainder.GetValueOrThrow());
        //We can't get the nested property as this is not an EntityStruct

        return
            Maybe<EntityProperty>.None;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<EntityProperty> GetEnumerator() => this.Headers.Zip(this.Values)
        .Select(((tuple, i) => new EntityProperty(tuple.First, tuple.Second, i)))
        .GetEnumerator();

    /// <inheritdoc />
    public string Serialize(SerializeOptions options)
    {
        var sb = new StringBuilder();

        sb.Append('(');

        var results = new List<string>();

        foreach (var (name, EntityStructValue, _) in this)
            results.Add($"'{name}': {EntityStructValue.Serialize(options)}");

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

            var longestPropertyName = Headers.Select(x => x.Length).Max();

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
    /// Create an EntityStruct from structured EntityStruct properties
    /// </summary>
    [Pure]
    private static ISCLObject CreateFromProperties(
        IReadOnlyList<(Maybe<EntityPropertyKey> key, ISCLObject argValue)> properties)
    {
        if (properties.Count == 0)
            return SCLNull.Instance;

        if (properties.Count == 1 && properties.Single().key.HasNoValue)
            return properties.Single().argValue;

        var entityStructProperties =
            new Dictionary<string, EntityProperty>(StringComparer.OrdinalIgnoreCase);

        void SetEntityProperty(string key, ISCLObject ev)
        {
            EntityProperty newProperty;

            if (entityStructProperties.TryGetValue(key, out var existingValue))
            {
                if (ev is Entity nestedEntityStruct)
                {
                    if (existingValue.Value is Entity existingNestedEntityStruct)
                    {
                        var nEntityStruct = existingNestedEntityStruct.Combine(nestedEntityStruct);

                        newProperty = new EntityProperty(
                            key,
                            nEntityStruct,
                            existingValue.Order
                        );
                    }
                    else
                    {
                        //Ignore the old property
                        newProperty = new EntityProperty(key, ev, existingValue.Order);
                    }
                }
                else if (existingValue.Value is Entity existingNestedEntityStruct)
                {
                    var nEntityStruct =
                        existingNestedEntityStruct.WithProperty(
                            Entity.PrimitiveKey,
                            ev
                        );

                    newProperty = new EntityProperty(
                        key,
                        nEntityStruct,
                        existingValue.Order
                    );
                }
                else //overwrite the existing property
                    newProperty = new EntityProperty(key, ev, existingValue.Order);
            }
            else //New property
                newProperty = new EntityProperty(key, ev, entityStructProperties.Count);

            entityStructProperties[key] = newProperty;
        }

        foreach (var (key, argValue) in properties)
        {
            if (key.HasNoValue)
            {
                if (argValue is Entity ne)
                    foreach (var ep in ne)
                        SetEntityProperty(ep.Name, ep.Value);
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

        //TODO improve performance
        var newEntityStruct = new Entity(
            entityStructProperties.Keys.ToImmutableArray(),
            entityStructProperties.Values.Select(x => x.Value).ToImmutableArray()
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
            var node = ep.Value.ToSchemaNode($"{path}/{ep.Name}", schemaConversionOptions);
            dictionary[ep.Name] = (node, true, order);
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
