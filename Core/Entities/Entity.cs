using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Serialization;

namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// A piece of data.
    /// </summary>
    public sealed class Entity : IEnumerable<KeyValuePair<string, EntityValue>>
    {
        private readonly ImmutableList<KeyValuePair<string, EntityValue>> _properties;

        /// <summary>
        /// The default property name if the Entity represents a single primitive.
        /// </summary>
        public const string PrimitiveKey = "value";

        /// <summary>
        /// Create a new entity
        /// </summary>
        public Entity(params KeyValuePair<string, EntityValue>[] properties) : this(properties.ToImmutableList()) {}

        /// <summary>
        /// Create a new entity.
        /// </summary>
        public Entity(ImmutableList<KeyValuePair<string, EntityValue>> properties) => _properties = properties;


        /// <summary>
        /// Create a new entity
        /// </summary>
        public static Entity Create(IEnumerable<KeyValuePair<string, object>> properties, char? multiValueDelimiter = null)
        {
            var propertyEntities = properties
                .Select(x => new KeyValuePair<string, EntityValue>(x.Key, EntityValue.Create(x.Value.ToString(), multiValueDelimiter)))
                .ToImmutableList();

            return new Entity(propertyEntities);
        }

        /// <summary>
        /// Gets the names of different properties on this object.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetPropertyNames() => _properties.Select(x => x.Key);



        /// <summary>
        /// Creates a copy of this with the property added or updated
        /// </summary>
        public Entity WithProperty(string key, EntityValue value)
        {
            var index = _properties.FindIndex(x => x.Key == key);
            if (index == -1)
            {
                var newList = _properties.Add(new KeyValuePair<string, EntityValue>(key, value));
                return new Entity(newList);
            }
            else
            {
                var newList = _properties.SetItem(index, new KeyValuePair<string, EntityValue>(key, value));
                return new Entity(newList);
            }
        }


        /// <summary>
        /// Try to get the value of a particular property
        /// </summary>
        public bool TryGetValue(string key, out EntityValue? entityValue)
        {
            var v = _properties.TryFirst(x => x.Key == key);
            if (v.HasValue)
            {
                entityValue = v.Value.Value;
                return true;
            }

            entityValue = null;
            return false;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, EntityValue>> GetEnumerator() => _properties.GetEnumerator();

        /// <inheritdoc />
        public override string ToString() => AsString();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        /// <summary>
        /// Serialize this record.
        /// </summary>
        /// <returns></returns>
        public Result<string> TrySerializeShortForm()
        {
            var sb = new StringBuilder();

            sb.Append("(");

            var results = new List<Result<string>>();

            foreach (var (key, value) in _properties)
            {
                value.Value.Switch(_=>{},
                    singleValue=>
                    {
                        var r = SerializationMethods.TrySerializeShortFormString(singleValue.Original)
                            .Map(v => $"{key} = {v}");
                        results.Add(r);
                    },
                    multiValue=>
                    {
                        var r = SerializationMethods.TrySerializeSimpleList(multiValue.Select(x => x.Original))
                            .Map(v => $"{key} = {v}");
                        results.Add(r);
                    });
            }

            var result = results.Combine();

            if (result.IsFailure)
                return result.ConvertFailure<string>();

            sb.AppendJoin(",", result.Value);

            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// Convert this entity to an object that can be serialized
        /// </summary>
        /// <returns></returns>
        public object ToSimpleObject()
        {
            IDictionary<string, object> expandoObject = new ExpandoObject();

            foreach (var (key, value) in _properties)
            {
                value.Value.Switch(_=>{},
                    v=> expandoObject[key] = v,
                    l => expandoObject[key] = l  );
            }

            return expandoObject;
        }


        /// <summary>
        /// Converts this record into a string.
        /// </summary>
        public string AsString()
        {
            var result = string.Join(", ",
                _properties.Select(property => $"{property.Key}: {property.Value}"));

            return result;
        }
    }
}
