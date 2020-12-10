using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
namespace Reductech.EDR.Core
{
    /// <summary>
    /// A piece of data.
    /// </summary>
    public sealed class Entity : IEnumerable<KeyValuePair<string, EntityValue>> , IEquatable<Entity>
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
                .Select(x => new KeyValuePair<string, EntityValue>(x.Key, EntityValue.CreateFromObject(x.Value, multiValueDelimiter)))
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
        public bool Equals(Entity? other)
        {
            return !(other is null) && _properties.SequenceEqual(other._properties);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is Entity e && Equals(e);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _properties.Count switch
            {
                0 => 0,
                1 => HashCode.Combine(_properties[0].Key, _properties[0].Value),
                _ => HashCode.Combine(_properties.Count, _properties[0].Key, _properties[0].Value)
            };
        }

        /// <inheritdoc />
        public override string ToString() => Serialize();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        /// <summary>
        /// Serialize this entity.
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            var sb = new StringBuilder();

            sb.Append('(');

            var results = new List<string>();

            foreach (var (key, value) in _properties)
            {
                if(!value.Value.IsT0)
                    results.Add($"{key}: {value.Serialize()}");
            }

            sb.AppendJoin(" ", results);

            sb.Append(')');

            var result = sb.ToString();

            return result;
        }

    }
}
