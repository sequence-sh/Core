using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;

// ReSharper disable once CheckNamespace - we want this namespace to prevent clash with FunctionalExtensions
namespace Reductech.EDR.Core
{
    /// <summary>
    /// A piece of data.
    /// </summary>
    public sealed class Entity : IEnumerable<EntityProperty> , IEquatable<Entity>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Entity(ImmutableDictionary<string, EntityProperty> dictionary) =>
            Dictionary = dictionary.WithComparers(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Constructor
        /// </summary>
        public Entity(IEnumerable<EntityProperty> entityProperties)
        {
            Dictionary = entityProperties.ToImmutableDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The dictionary of property values.
        /// </summary>
        public ImmutableDictionary<string, EntityProperty> Dictionary { get; }

        /// <summary>
        /// The default property name if the Entity represents a single primitive.
        /// </summary>
        public const string PrimitiveKey = "value";

        /// <summary>
        /// Creates a new Entity
        /// </summary>
        public static Entity Create(params (string key, object? property)[] properties) => Create(properties.AsEnumerable());

        /// <summary>
        /// Creates a new Entity
        /// </summary>
        public static Entity Create(IEnumerable<(string key, object? property)> properties, char? multiValueDelimiter = null)
        {
            var dict = properties.Select((x, i) =>
                    new EntityProperty(x.key, EntityValue.CreateFromObject(x.property, multiValueDelimiter), null, i))
                .ToImmutableDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            return new Entity(dict);
        }


        /// <summary>
        /// Creates a copy of this with the property added or updated
        /// </summary>
        public Entity WithProperty(string key, EntityValue newValue)
        {
            ImmutableDictionary<string, EntityProperty> newDict;

            if (Dictionary.TryGetValue(key, out var ep))
            {
                var newProperty = new EntityProperty(ep.Name, newValue, null, ep.Order);
                newDict = Dictionary.SetItem(key, newProperty);
            }
            else
            {
                var property = new EntityProperty(key, newValue, null, Dictionary.Count);
                newDict = Dictionary.Add(key, property);
            }

            return new Entity(newDict);
        }


        /// <summary>
        /// Try to get the value of a particular property
        /// </summary>
        public Maybe<EntityValue> TryGetValue(string key)
        {
            if (Dictionary.TryGetValue(key, out var ep))
                return ep.NewValue ?? ep.BaseValue;

            return Maybe<EntityValue>.None;
        }

        /// <inheritdoc />
        public IEnumerator<EntityProperty> GetEnumerator() => Dictionary.Values.OrderBy(x=>x.Order).GetEnumerator();

        /// <inheritdoc />
        public bool Equals(Entity? other)
        {
            return !(other is null) && Dictionary.Values.SequenceEqual(other.Dictionary.Values);
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
            return Dictionary.Count switch
            {
                0 => 0,
                1 => Dictionary.Single().Value.GetHashCode(),
                _ => HashCode.Combine(Dictionary.Count, Dictionary.First().Value.GetHashCode())
            };
        }

        /// <inheritdoc />
        public override string ToString() => this.Serialize();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Contains methods for helping with entities.
    /// </summary>
    public static class EntityHelper
    {
        /// <summary>
        /// Tries to convert an object into one suitable as an entity property.
        /// </summary>
        public static async Task<Result<object?, IError>> TryUnpackObjectAsync(object? o, CancellationToken cancellation)
        {
            if (o is ISequence list)
            {
                var r = await list.GetObjectsAsync(cancellation);

                if (r.IsFailure) return r.ConvertFailure<object?>();

                var q = await r.Value.Select(x => TryUnpackObjectAsync(x, cancellation))
                    .Combine(ErrorList.Combine).Map(x => x.ToList());

                return q;
            }

            return Result.Success<object?, IError>(o);
        }
    }
}
