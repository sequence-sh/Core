using System;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;

using Option = OneOf.OneOf<string, int, double, bool, Reductech.EDR.Core.Internal.Enumeration, System.DateTime, Reductech.EDR.Core.Entity>;

namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// The value of a single-value entity, or one of the values of a multi-value entity.
    /// </summary>
    public class EntitySingleValue : IEquatable<EntitySingleValue>
    {
        /// <summary>
        /// Create a new EntitySingleValue
        /// </summary>
        public EntitySingleValue(Option value, string original)
        {
            Value = value;
            Original = original;
        }

        /// <summary>
        /// Create a new EntitySingleValue with a string property.
        /// </summary>
        public static EntitySingleValue Create(string s) => new EntitySingleValue(Option.FromT0(s), s);

        /// <summary>
        /// The original string
        /// </summary>
        public string Original { get; }

        /// <summary>
        /// The value
        /// </summary>
        public Option Value { get; }

        /// <summary>
        /// The type of the value
        /// </summary>
        public SchemaPropertyType Type => Value.Match(
            _ => SchemaPropertyType.String,
            _ => SchemaPropertyType.Integer,
            _ => SchemaPropertyType.Double,
            _ => SchemaPropertyType.Bool,
            _ => SchemaPropertyType.Enum,
            _ => SchemaPropertyType.Date,
            _ => SchemaPropertyType.Entity
        );

        /// <summary>
        /// Returns whether this value obeys the schema already without conversion.
        /// </summary>
        public bool Obeys(SchemaProperty schemaProperty)
        {
            if (Type != schemaProperty.Type)
                return false;


            if (schemaProperty.Type == SchemaPropertyType.Enum) //Is the value a valid type for this enum.
                return schemaProperty.Format != null && schemaProperty.Format.Contains(Original);


            if (schemaProperty.Regex != null)
            {
                if (!Regex.IsMatch(Original, schemaProperty.Regex))
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Try to convert this EntityValue to the type of the schemaProperty.
        /// </summary>
        public Result<EntitySingleValue, IErrorBuilder> TryConvert(SchemaProperty schemaProperty)
        {
            if (Obeys(schemaProperty))
                return this;

            if (schemaProperty.Regex != null)
            {
                if (!Regex.IsMatch(Original, schemaProperty.Regex))
                    return new ErrorBuilder($"'{Original}' does not match regex '{schemaProperty.Regex}'", ErrorCode.SchemaViolation);
            }

            var r = ConvertTo(Original, schemaProperty);

            if (r.HasNoValue)
                return new ErrorBuilder($"Could not convert '{Original}' to {schemaProperty.Type}", ErrorCode.SchemaViolation);

            return r.Value;
        }


        private static Maybe<EntitySingleValue> ConvertTo(string original, SchemaProperty schemaProperty)
        {
            var r = schemaProperty.Type switch
            {
                SchemaPropertyType.String => new EntitySingleValue(original, original),
                SchemaPropertyType.Integer => int.TryParse(original, out var i) ? new EntitySingleValue(i, original) :Maybe<EntitySingleValue>.None,
                SchemaPropertyType.Double => double.TryParse(original, out var d) ? new EntitySingleValue(d, original) : Maybe<EntitySingleValue>.None,
                SchemaPropertyType.Enum => schemaProperty.Format != null && schemaProperty.Format.Contains(original, StringComparer.OrdinalIgnoreCase) ? new EntitySingleValue(new Enumeration("Enum", original),original ) : Maybe<EntitySingleValue>.None, //TODO fix enum type
                SchemaPropertyType.Bool => bool.TryParse(original, out var b) ? new EntitySingleValue(b, original) : Maybe<EntitySingleValue>.None,
                SchemaPropertyType.Date => DateTime.TryParse(original, out var dt) ? new EntitySingleValue(dt, original) : Maybe<EntitySingleValue>.None, //TODO format
                SchemaPropertyType.Entity => Maybe<EntitySingleValue>.None, //TODO allow this
                _ => throw new ArgumentOutOfRangeException(nameof(schemaProperty))
            };
            return r;
        }

        /// <summary>
        /// This value as a string
        /// </summary>
        public string GetStringValue(string dateFormat)
        {
            return Value.Match(x => x,
                x => x.ToString(),
                x => x.ToString("G17"),
                x => x.ToString(),
                x => x.ToString(),
                x => x.ToString(dateFormat),
                x=>x.ToString());
        }

        /// <inheritdoc />
        public override string ToString() => GetStringValue("yyyy/MM/dd H:mm:ss");

        /// <inheritdoc />
        public bool Equals(EntitySingleValue? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Original == other.Original;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EntitySingleValue) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => Original.GetHashCode();

        /// <summary>
        /// Serialize this entity
        /// </summary>
        public string Serialize()
        {
            return Value.Match(SerializationMethods.DoubleQuote,
                x => x.ToString(),
                x => x.ToString("G17"),
                x => x.ToString(),
                x => x.ToString(),
                x => x.ToString("O"),
                x => x.Serialize());
        }
    }
}