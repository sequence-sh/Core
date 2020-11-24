using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// An entity schema.
    /// Enforces that the entity matches certain constraints.
    /// </summary>
    public sealed class Schema
    {

        /// <summary>
        /// The name of the schema.
        /// </summary>
        [YamlMember]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The schema properties.
        /// </summary>
        [YamlMember]
        public Dictionary<string, SchemaProperty> Properties { get; set; } = null!; //public setter for deserialization

        /// <summary>
        /// Whether properties other than the explicitly defined properties are allowed.
        /// </summary>
        [YamlMember]
        public bool AllowExtraProperties { get; set; } = true;

        /// <inheritdoc />
        public override string ToString()
        {
            // ReSharper disable once ConstantNullCoalescingCondition
            return Name??"Schema";
        }

        /// <summary>
        /// Attempts to apply this schema to an entity.
        /// </summary>
        public Result<Entity, IErrorBuilder> ApplyToEntity(Entity entity)
        {
            var remainingProperties = Properties
                .ToDictionary(x=>x.Key,
                    x=>x.Value,
                    StringComparer.OrdinalIgnoreCase);

            var keyValuePairs = new List<KeyValuePair<string, EntityValue>>();
            var errors = new List<IErrorBuilder>();

            foreach (var kvp in entity)
            {
#pragma warning disable 8714
                if (remainingProperties.Remove(kvp.Key, out var schemaProperty))
#pragma warning restore 8714
                {
                    var r = kvp.Value.TryConvert(schemaProperty);
                    if(r.IsSuccess)
                        keyValuePairs.Add(new KeyValuePair<string, EntityValue>(kvp.Key, r.Value));
                    else
                        errors.Add(r.Error);
                }
                else if (AllowExtraProperties)
                    keyValuePairs.Add(kvp);
                else
                    errors.Add(new ErrorBuilder($"Unexpected Property '{kvp.Key}'", ErrorCode.SchemaViolation));
            }

            foreach (var (key, _) in remainingProperties
                .Where(x=>x.Value.Multiplicity == Multiplicity.ExactlyOne || x.Value.Multiplicity == Multiplicity.AtLeastOne))
                errors.Add(new ErrorBuilder($"Missing property '{key}'", ErrorCode.SchemaViolation));

            if (errors.Any())
            {
                var l = ErrorBuilderList.Combine(errors);
                return Result.Failure<Entity, IErrorBuilder>(l);
            }


            var resultEntity = new Entity(keyValuePairs);

            return resultEntity;
        }
    }

    /// <summary>
    /// The value of an entity property.
    /// </summary>
    public class EntityValue
    {
        /// <summary>
        /// Create a new entityValue
        /// </summary>
        /// <param name="value"></param>
        public EntityValue(OneOf<DBNull, EntitySingleValue, IReadOnlyCollection<EntitySingleValue>> value) => Value = value;

        /// <summary>
        /// Create a new EntityValue from an original string.
        /// </summary>
        public static EntityValue Create(string? original)
        {
            if(string.IsNullOrWhiteSpace(original))
                return new EntityValue(DBNull.Value);

            return new EntityValue(EntitySingleValue.Create(original));
        }

        /// <summary>
        /// Create a new EntityValue from some number of original strings.
        /// </summary>
        public static EntityValue Create(IEnumerable<string> originals)
        {
            var values = originals.Select(EntitySingleValue.Create).ToList();

            return values.Count switch
            {
                0 => new EntityValue(DBNull.Value),
                1 => new EntityValue(values.Single()),
                _ => new EntityValue(values)
            };
        }

        /// <summary>
        /// The Value
        /// </summary>
        public OneOf<DBNull, EntitySingleValue, IReadOnlyCollection<EntitySingleValue>> Value { get; }

        /// <summary>
        /// Tries to convert the value so it matches the schema.
        /// </summary>
        public Result<EntityValue, IErrorBuilder> TryConvert(SchemaProperty schemaProperty)
        {
            var r =

            Value.Match(_ =>
                {
                    if (schemaProperty.Multiplicity == Multiplicity.Any ||
                        schemaProperty.Multiplicity == Multiplicity.UpToOne)
                        return Result.Success<EntityValue, IErrorBuilder>(this);
                    return new ErrorBuilder("Unexpected null", ErrorCode.SchemaViolation);


                },
                singleValue =>
                {
                    if (singleValue.Type == schemaProperty.Type && singleValue.Obeys(schemaProperty))
                        return this;
                    return singleValue.TryConvert(schemaProperty).Map(x => new EntityValue(x));
                },
                multiValue =>
                {
                    if (schemaProperty.Multiplicity == Multiplicity.Any || schemaProperty.Multiplicity == Multiplicity.AtLeastOne)
                    {
                        if(multiValue.All(x=>x.Type == schemaProperty.Type && x.Obeys(schemaProperty)))
                            return Result.Success<EntityValue, IErrorBuilder>(this);

                        var result = multiValue.Select(x=>x.TryConvert(schemaProperty))
                                .Combine(ErrorBuilderList.Combine)
                                .Map(x => new EntityValue(x.ToList()));

                        return result;
                    }

                    return new ErrorBuilder("Unexpected list", ErrorCode.SchemaViolation);
                });

            return r;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value.Match(x => "Empty", x => x.ToString(), x => string.Join(", ", x));
        }
    }

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
        /// Try to convert this EntityValue to they type of the schemaProperty.
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

    /// <summary>
    /// A single property in a a schema.
    /// </summary>
    public sealed class SchemaProperty
    {
        /// <summary>
        /// The type of the property.
        /// </summary>
        [YamlMember]
        public SchemaPropertyType Type { get; set; }

        /// <summary>
        /// The multiplicity of the property.
        /// </summary>
        [YamlMember]
        public Multiplicity Multiplicity { get; set; } = Multiplicity.Any;

        /// <summary>
        /// The format strings.
        /// For Date, this will contain possible date formats.
        /// For Enum, this will contain possible enum values.
        /// </summary>
        [YamlMember]
        public List<string>? Format { get; set; }

        /// <summary>
        /// A regex to validate the string form of the field value
        /// </summary>
        public string? Regex { get; set; }

    }


    /// <summary>
    /// The multiplicity of the property
    /// </summary>
    public enum Multiplicity
    {
        /// <summary>
        /// Any number of values - a list
        /// </summary>
        Any,
        /// <summary>
        /// At least one value - a non-empty list
        /// </summary>
        AtLeastOne,
        /// <summary>
        /// Exactly one value
        /// </summary>
        ExactlyOne,
        /// <summary>
        /// Either one or zero values
        /// </summary>
        UpToOne
    }

    /// <summary>
    /// The type of the property
    /// </summary>
    public enum SchemaPropertyType
    {
        /// <summary>
        /// A string.
        /// </summary>
        String,
        /// <summary>
        /// An integer.
        /// </summary>
        Integer,
        /// <summary>
        /// A double precision number.
        /// </summary>
        Double,
        /// <summary>
        /// An enumeration of some sort.
        /// The format string will contain the possible values.
        /// </summary>
        Enum,
        /// <summary>
        /// A boolean.
        /// </summary>
        Bool,
        /// <summary>
        /// A date.
        /// </summary>
        Date

    }
}
