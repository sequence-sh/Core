﻿using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;

using Option = OneOf.OneOf<System.DBNull, string, int, double, bool, Reductech.EDR.Core.Internal.Enumeration, System.DateTime, Reductech.EDR.Core.Entity, System.Collections.Immutable.ImmutableList<Reductech.EDR.Core.Entities.EntityValue>>;


namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// The value of an entity property.
    /// </summary>
    public sealed class EntityValue : IEquatable<EntityValue>
    {
        /// <summary>
        /// Create a new entityValue
        /// </summary>
        /// <param name="value"></param>
        public EntityValue(Option value) => Value = value;

        /// <summary>
        /// The Value
        /// </summary>
        public Option Value { get; }

        /// <summary>
        /// Create an entity from an object
        /// </summary>
        public static EntityValue CreateFromObject(object? argValue, char? multiValueDelimiter = null)
        {
            if (argValue == null) //TODO convert to switch statement (this is not supported by stryker at the moment)
            {
                return new EntityValue(DBNull.Value);
            }
            else if (argValue is StringStream ss1)
            {
                return Create(ss1.GetString(), multiValueDelimiter);
            }
            else if (argValue is string s)
            {
                return Create(s, multiValueDelimiter);
            }
            else if (argValue is int i)
            {
                return new EntityValue(i);
            }
            else if (argValue is bool b)
            {
                return new EntityValue(b);
            }
            else if (argValue is double d)
            {
                return new EntityValue(d);
            }
            else if (argValue is Enumeration e1)
            {
                return new EntityValue(e1);
            }
            else if (argValue is DateTime dt)
            {
                return new EntityValue(dt);
            }
            else if (argValue is Entity entity)
            {
                return new EntityValue(entity);
            }
            else if (argValue is IEnumerable e2)
            {
                var newEnumerable = e2.Cast<object>().Select(v=>CreateFromObject(v, multiValueDelimiter)).ToImmutableList();
                if(!newEnumerable.Any())
                    return new EntityValue(DBNull.Value);
                return new EntityValue(newEnumerable);
            }
            else if (argValue is IResult)
            {
                throw new ArgumentException(
                    "Attempt to set EntityValue to a Result - you should check the result for failure and then set it to the value of the result",
                    nameof(argValue));
            }
            else
            {
                return new EntityValue(argValue.ToString()!);
            }

            static EntityValue Create(string s, char? multiValueDelimiter)
            {
                if (multiValueDelimiter == null)
                    return new EntityValue(s);

                var stringArray = s.Split(multiValueDelimiter.Value);
                if (stringArray.Length > 1)
                    return new EntityValue(stringArray.Select(s => new EntityValue(s)).ToImmutableList());

                return new EntityValue(s);
            }
        }

        /// <summary>
        /// Tries to convert the value so it matches the schema.
        /// </summary>
        public Result<(EntityValue value, bool changed), IErrorBuilder> TryConvert(SchemaProperty schemaProperty)
        {
            if(schemaProperty.Regex!= null)
            {
                var s = this.GetString();

                if (!Regex.IsMatch(s, schemaProperty.Regex))
                    return new ErrorBuilder(@$"'{s}' does not match regex '{schemaProperty.Regex}'", ErrorCode.SchemaViolation);
            }

            ErrorBuilder CouldNotConvert(object o) =>
                new ErrorBuilder($"Could not convert '{o}' to {schemaProperty.Type}", ErrorCode.SchemaViolation);


            var r = Value.Match(
                MatchDbNull,
                MatchString,
                MatchInt,
                MatchDouble,
                MatchBool,
                MatchEnum,
                MatchDateTime,
                MatchEntity,
                MatchList);

            return r;


            Result<(EntityValue value, bool changed), IErrorBuilder> MatchDbNull(DBNull dbNull)
            {
                if (schemaProperty.Multiplicity == Multiplicity.Any ||
                            schemaProperty.Multiplicity == Multiplicity.UpToOne)
                    return (this, false);
                return new ErrorBuilder("Unexpected null", ErrorCode.SchemaViolation);
            }

            Result<(EntityValue value, bool changed) , IErrorBuilder> MatchString(string s)
            {
                switch (schemaProperty.Type)
                {
                    case SchemaPropertyType.String: return (this, false);
                    case SchemaPropertyType.Integer:
                    {
                        if (int.TryParse(s, out var i))
                            return (new EntityValue(i), true);
                        break;
                    }
                    case SchemaPropertyType.Double:
                        {
                            if (double.TryParse(s, out var d))
                                return (new EntityValue(d), true);
                            break;
                        }
                    case SchemaPropertyType.Enum:
                        {
                            if(string.IsNullOrWhiteSpace(schemaProperty.EnumType))
                                return new ErrorBuilder("Schema does not define the name of the enum", ErrorCode.SchemaViolation);
                            if(schemaProperty.Format == null || !schemaProperty.Format.Any())
                                return new ErrorBuilder("Schema does not define any possible values for the enum", ErrorCode.SchemaViolation);

                            if(schemaProperty.Format.Contains(s, StringComparer.OrdinalIgnoreCase))
                                return (new EntityValue(new Enumeration(schemaProperty.EnumType, s)), true);
                            break;
                        }
                    case SchemaPropertyType.Bool:
                        {
                            if (bool.TryParse(s, out var b))
                                return (new EntityValue(b), true);
                            break;
                        }
                    case SchemaPropertyType.Date:
                        {
                            if (DateTime.TryParse(s, out var dt)) //TODO format
                                return (new EntityValue(dt), true);
                            break;
                        }
                    default:
                        return CouldNotConvert(s);
                }
                return CouldNotConvert(s);
            }

            Result<(EntityValue value, bool changed), IErrorBuilder> MatchInt(int i)
            {
                return schemaProperty.Type switch
                {
                    SchemaPropertyType.String => (new EntityValue(i.ToString()), true),
                    SchemaPropertyType.Integer => (this, false),
                    SchemaPropertyType.Double => (new EntityValue(Convert.ToDouble(i)), true),
                    _ => CouldNotConvert(i)
                };
            }

            Result<(EntityValue value, bool changed), IErrorBuilder> MatchDouble(double d)
            {
                return schemaProperty.Type switch
                {
                    SchemaPropertyType.String => (new EntityValue(d.ToString("G17")), false),
                    SchemaPropertyType.Double => (this, true),
                    _ => CouldNotConvert(d)
                };
            }

            Result<(EntityValue value, bool changed), IErrorBuilder> MatchBool(bool b)
            {
                return schemaProperty.Type switch
                {
                    SchemaPropertyType.String => (new EntityValue(b.ToString()), true),
                    SchemaPropertyType.Bool => (this, false),
                    _ => CouldNotConvert(b)
                };
            }

            Result<(EntityValue value, bool changed), IErrorBuilder> MatchEnum(Enumeration e)
            {
                switch (schemaProperty.Type)
                {
                    case SchemaPropertyType.String: return (this, false);
                    case SchemaPropertyType.Enum:
                        {
                            if (schemaProperty.EnumType == null)
                                return new ErrorBuilder("Schema does not define the name of the enum", ErrorCode.SchemaViolation);
                            if (schemaProperty.Format == null || !schemaProperty.Format.Any())
                                return new ErrorBuilder("Schema does not define any possible values for the enum", ErrorCode.SchemaViolation);

                            if (schemaProperty.Format.Contains(e.Value, StringComparer.OrdinalIgnoreCase))
                            {
                                if (schemaProperty.EnumType == e.Type)
                                    return (this, false);
                                return (new EntityValue(new Enumeration(schemaProperty.EnumType, e.Value)), true);
                            }

                            return CouldNotConvert(e);
                        }
                    default:
                        return CouldNotConvert(e);
                }
            }

            Result<(EntityValue value, bool changed), IErrorBuilder> MatchDateTime(DateTime dt)
            {
                return schemaProperty.Type switch
                {
                    SchemaPropertyType.String => (new EntityValue(dt.ToString("O")), true),
                    SchemaPropertyType.Date => (this, false),
                    _ => CouldNotConvert(dt)
                };
            }
            Result<(EntityValue value, bool changed), IErrorBuilder> MatchEntity(Entity e)
            {
                return schemaProperty.Type switch
                {
                    SchemaPropertyType.String => (new EntityValue(e.ToString()), true),
                    SchemaPropertyType.Enum => (this, false),
                    _ => CouldNotConvert(e)
                };
            }

            Result<(EntityValue value, bool changed), IErrorBuilder> MatchList(ImmutableList<EntityValue> list)
            {
                if (schemaProperty.Multiplicity == Multiplicity.UpToOne ||
                    schemaProperty.Multiplicity == Multiplicity.ExactlyOne)
                {
                    if (list.Count == 1)
                        return list.Single().TryConvert(schemaProperty);
                    else if (list.Count == 0 && schemaProperty.Multiplicity == Multiplicity.UpToOne)
                        return (new EntityValue(DBNull.Value), true);

                    return new ErrorBuilder($"Unexpected list with {list.Count} elements", ErrorCode.SchemaViolation);
                }


                var sp = new SchemaProperty()
                {
                    EnumType = schemaProperty.EnumType,
                    Format = schemaProperty.Format,
                    Multiplicity = Multiplicity.ExactlyOne,
                    Regex = schemaProperty.Regex,
                    Type = schemaProperty.Type
                };

                var newList = list.Select(x => x.TryConvert(sp))
                    .Combine(ErrorBuilderList.Combine)
                    .Map(x=>x.Aggregate(
                        (list: ImmutableList<EntityValue>.Empty, changed: false),
                        (a, b) => (a.list.Add(b.value), a.changed || b.changed)))
                    .Map(x=>
                        x.changed?(new EntityValue(x.list), true) : (this, false));
                return newList;
            }
        }

        /// <inheritdoc />
        public bool Equals(EntityValue? other)
        {
            return !(other is null)  && Value.Equals(other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is EntityValue ev && Equals(ev);
        }

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode(); //TODO get hash code from immutable list

        /// <inheritdoc />
        public override string ToString()
        {
            return Value.Match(_ => "Empty",
                x => x,
                x => x.ToString(),
                x => x.ToString("G17"),
                x => x.ToString(),
                x => x.ToString(),
                x => x.ToString("O"),
                x => x.ToString(),
                x => x.Count + " elements");
        }

    }
}