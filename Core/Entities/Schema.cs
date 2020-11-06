using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// An entity schema.
    /// Enforces that the entity matches certain constraints.
    /// </summary>
    public sealed class Schema
    {
    }

    //public class EntityValue : Option<DBNull,IReadOnlyCollection<EntitySingleValue>, EntitySingleValue>
    //{
    //    //TODO original string value


    //    /// <inheritdoc />
    //    public EntityValue(DBNull t1) : base(t1) {}

    //    /// <inheritdoc />
    //    public EntityValue(IReadOnlyCollection<EntitySingleValue> t2) : base(t2) {}

    //    /// <inheritdoc />
    //    public EntityValue(EntitySingleValue t3) : base(t3) {}
    //}

    //public class EntitySingleValue : Option<int, double, bool, string, DateTime>
    //{

    //}

    /// <summary>
    /// A single property in a a schema.
    /// </summary>
    public sealed class SchemaProperty
    {
        /// <summary>
        /// Ensures that this property meets the constraints of the schema.
        /// </summary>
        public Result<object?, IErrorBuilder> Enforce(object? currentValue)
        {
            if (currentValue == null)
            {
                if (Multiplicity.AllowsNull())
                    return currentValue;
                return new ErrorBuilder("Unexpected null", ErrorCode.SchemaViolation);
            }
            else if (currentValue is IEnumerable enumerable)
            {
                if (Multiplicity.AllowsList())
                {
                    var result =
                    enumerable.OfType<object>().Select(TryConvertSingleValue)
                        .Combine(ErrorBuilderList.Combine)
                        .Map(x => x.ToList() as object);

                    return result;
                }
                else
                {
                    return new ErrorBuilder("Unexpected list", ErrorCode.SchemaViolation);
                }
            }
            else
            {
                return TryConvertSingleValue(currentValue);
            }
        }


        private static Maybe<int> TryConvertToInt(object currentValue)
        {
            return currentValue switch
            {
                int i => i,
                double d => Convert.ToInt32(d),
                bool b => Convert.ToInt32(b),
                string s when int.TryParse(s, out var castInt) => castInt,
                _ => Maybe<int>.None
            };
        }

        private static Maybe<double> TryConvertToDouble(object currentValue)
        {
            return currentValue switch
            {
                int i => i,
                double d => d,
                bool b => Convert.ToDouble(b),
                string s when double.TryParse(s, out var castDouble) => castDouble,
                _ => Maybe<double>.None
            };
        }

        private static Maybe<bool> TryConvertToBool(object currentValue)
        {
            return currentValue switch
            {
                int i when i == 1 => true,
                int i when i == 0 => false,
                bool b => b,
                string s when bool.TryParse(s, out var castBool) => castBool,
                _ => Maybe<bool>.None
            };
        }

        private Maybe<string> TryConvertToEnum(object currentValue)
        {
            var s = currentValue.ToString();

            if (Format.Contains(s))
                return s!;
            return Maybe<string>.None;
        }

        private Maybe<DateTime> TryConvertToDateTime(object currentValue)
        {
            switch (currentValue)
            {
                case DateTime dt:
                    return dt;
                case string s:
                {
                    //TODO formats
                    if(DateTime.TryParse(s, out var result))
                        return result;
                    return Maybe<DateTime>.None;
                }

                default:
                    return Maybe<DateTime>.None;
            }
        }

        private Result<object, IErrorBuilder> TryConvertSingleValue(object currentValue)
        {
            var r = SchemaPropertyType switch
            {
                SchemaPropertyType.String => Maybe<object>.From(currentValue.ToString()!),
                SchemaPropertyType.Integer => TryConvertToInt(currentValue),
                SchemaPropertyType.Double => TryConvertToDouble(currentValue),
                SchemaPropertyType.Enum => TryConvertToEnum(currentValue),
                SchemaPropertyType.Bool => TryConvertToBool(currentValue),
                SchemaPropertyType.Date => TryConvertToDateTime(currentValue),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (r.HasValue)
                return r.Value;

            return new ErrorBuilder($"Could not convert '{currentValue}' to {SchemaPropertyType}", ErrorCode.SchemaViolation);
        }


        /// <summary>
        /// Create a new SchemaProperty
        /// </summary>
        public SchemaProperty(SchemaPropertyType schemaPropertyType, Multiplicity multiplicity, IReadOnlyCollection<string> format)
        {
            SchemaPropertyType = schemaPropertyType;
            Multiplicity = multiplicity;
            Format = format;
        }

        /// <summary>
        /// The type of the property.
        /// </summary>
        public SchemaPropertyType SchemaPropertyType { get; }

        /// <summary>
        /// The multiplicity of the property.
        /// </summary>
        public Multiplicity Multiplicity { get; }

        /// <summary>
        /// The format strings.
        /// For Date, this will contain possible date formats.
        /// For Enum, this will contain possible enum values.
        /// </summary>
        public IReadOnlyCollection<string> Format { get; }

    }

    /// <summary>
    /// Extension methods to help with schema.
    /// </summary>
    public static class SchemaHelper
    {
        public static bool AllowsNull(this Multiplicity multiplicity) => multiplicity == Multiplicity.Any || multiplicity == Multiplicity.UpToOne;

        public static bool AllowsList(this Multiplicity multiplicity) => multiplicity == Multiplicity.Any || multiplicity == Multiplicity.AtLeastOne;
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
