using System;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

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
        public EntitySingleValue(OneOf<string, int, double, bool, string, DateTime> value, string original)
        {
            Value = value;
            Original = original;
        }

        /// <summary>
        /// Create a new EntitySingleValue with a string property.
        /// </summary>
        public static EntitySingleValue Create(string s) => new EntitySingleValue(OneOf<string, int, double, bool, string, DateTime>.FromT0(s), s);

        /// <summary>
        /// The original string
        /// </summary>
        public string Original { get; }

        /// <summary>
        /// The value
        /// </summary>
        public OneOf<string, int, double, bool, string, DateTime> Value { get; }

        /// <summary>
        /// The type of the value
        /// </summary>
        public SchemaPropertyType Type => Value.Match(
            _ => SchemaPropertyType.String,
            _ => SchemaPropertyType.Integer,
            _ => SchemaPropertyType.Double,
            _ => SchemaPropertyType.Bool,
            _ => SchemaPropertyType.Enum,
            _ => SchemaPropertyType.Date
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

        ///// <summary>
        ///// Attempt to convert this value to a particular type
        ///// </summary>
        //public Maybe<T> TryConvertTo<T>()
        //{
        //    if (Original is T tString) return tString;
        //    if (DateTime.Now is T)
        //    {
        //        if (Value.IsT5 && Value.AsT5 is T tDateTime) return tDateTime;
        //        if (DateTime.TryParse(Original, out var dt) && dt is T tDateTime2) return tDateTime2;

        //    }

        //    else if (2.0 is T)
        //    {
        //        if (Value.IsT2 && Value.AsT2 is T tDouble) return tDouble;
        //        if (Value.IsT1 && Value.AsT1 is T tDouble2) return tDouble2;
        //        if (double.TryParse(Original, out var d) && d is T tDouble3) return tDouble3;
        //    }

        //    else if (2 is T)
        //    {
        //        if (Value.IsT1 && Value.AsT1 is T tint) return tint;
        //        if (int.TryParse(Original, out var i) && i is T tint2) return tint2;
        //    }
        //    else if (true is T)
        //    {
        //        if (Value.IsT3 && Value.AsT3 is T tBool) return tBool;
        //        if (bool.TryParse(Original, out var b) && b is T tBool2) return tBool2;
        //    }
        //    else if (typeof(T).IsEnum)
        //    {
        //        if (Value.IsT4 && Enum.TryParse(typeof(T), Value.AsT4, true, out var e) && e is T tEnum) return tEnum;
        //        if (Enum.TryParse(typeof(T), Original, true, out var e2) && e2 is T tEnum2) return tEnum2;
        //    }

        //    return Maybe<T>.None;
        //}


        private static Maybe<EntitySingleValue> ConvertTo(string original, SchemaProperty schemaProperty)
        {
            var r = schemaProperty.Type switch
            {
                SchemaPropertyType.String => new EntitySingleValue(OneOf<string, int, double, bool, string, DateTime>.FromT0(original), original),
                SchemaPropertyType.Integer => int.TryParse(original, out var i) ? new EntitySingleValue(i, original) :Maybe<EntitySingleValue>.None,
                SchemaPropertyType.Double => double.TryParse(original, out var d) ? new EntitySingleValue(d, original) : Maybe<EntitySingleValue>.None,
                SchemaPropertyType.Enum => schemaProperty.Format != null && schemaProperty.Format.Contains(original, StringComparer.OrdinalIgnoreCase) ? new EntitySingleValue(OneOf<string, int, double, bool, string, DateTime>.FromT4(original),original ) : Maybe<EntitySingleValue>.None,
                SchemaPropertyType.Bool => bool.TryParse(original, out var b) ? new EntitySingleValue(b, original) : Maybe<EntitySingleValue>.None,
                SchemaPropertyType.Date => DateTime.TryParse(original, out var dt) ? new EntitySingleValue(dt, original) : Maybe<EntitySingleValue>.None, //TODO format
                _ => throw new ArgumentOutOfRangeException()
            };
            return r;
        }

        /// <inheritdoc />
        public override string ToString() => Original;

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
    }
}