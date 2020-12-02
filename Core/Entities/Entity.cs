using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public override string ToString() => Serialize();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        /// <summary>
        /// Serialize this record.
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            var sb = new StringBuilder();

            sb.Append('(');

            var results = new List<string>();

            foreach (var (key, value) in _properties)
            {
                value.Value.Switch(_=>{},
                    singleValue=>
                    {
                        var v = SerializationMethods.EscapeDoubleQuotes(singleValue.Original);
                        results.Add($"{key} = {v}");

                    },
                    multiValue=>
                    {
                        var v = multiValue.Select(x => x.Original)
                            .Select(SerializationMethods.EscapeDoubleQuotes);

                        var r = SerializationMethods.SerializeList(v);
                        results.Add(r);
                    });
            }

            sb.AppendJoin(",", results);

            sb.Append(')');

            return sb.ToString();
        }

    }
}
