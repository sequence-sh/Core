using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
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

        /// <summary>
        /// The name of the schema.
        /// </summary>
        [ConfigProperty(Order = 1)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The schema properties.
        /// </summary>
        [ConfigProperty(Order = 2)]
        public Dictionary<string, SchemaProperty> Properties { get; set; } = null!; //public setter for deserialization

        /// <summary>
        /// Whether properties other than the explicitly defined properties are allowed.
        /// </summary>
        [ConfigProperty(Order = 3)]
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

            var keyValuePairs = ImmutableList<KeyValuePair< string,EntityValue>>.Empty.ToBuilder();
            var errors = new List<IErrorBuilder>();

            foreach (var kvp in entity)
            {
                if (remainingProperties.Remove(kvp.Key, out var schemaProperty))
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


            var resultEntity = new Entity(keyValuePairs.ToImmutable());

            return resultEntity;
        }

        /// <summary>
        /// Tries to create a schema from an entity.
        /// Ignores unexpected properties.
        /// </summary>
        public static Result<Schema, IErrorBuilder> TryCreateFromEntity(Entity entity)
        {
            var results = new List<Result<Unit, IErrorBuilder>>();
            var schema = new Schema();

            results.Add(entity.TrySetString(nameof(Name), nameof(Schema), s => schema.Name = s));
            results.Add(entity.TrySetBoolean(nameof(AllowExtraProperties), nameof(Schema), s => schema.AllowExtraProperties = s));

            results.Add(entity.TrySetDictionary(nameof(Properties), nameof(Schema), ev =>
            {
                if(ev.Value.IsT1 && ev.Value.AsT1.Value.IsT6)
                    return SchemaProperty.TryCreateFromEntity(ev.Value.AsT1.Value.AsT6);
                else
                    return new ErrorBuilder($"Could not convert {ev} to SchemaProperty", ErrorCode.InvalidCast);
            }, d=> schema.Properties = d));

            var r = results.Combine(ErrorBuilderList.Combine)
                .Map(_ => schema);


            return r;

        }

        /// <summary>
        /// Converts a schema to an entity for deserialization
        /// </summary>
        public Entity ConvertToEntity()
        {
            var propertiesEntity = new Entity(
                Properties.Select(x=>
                    new KeyValuePair<string,EntityValue>(x.Key, EntityValue.CreateFromObject(x.Value.ConvertToEntity()))).ToImmutableList()
            );


            var topProperties = new []
            {
                new KeyValuePair<string, EntityValue>(nameof(Name), EntityValue.CreateFromObject(Name)),
                new KeyValuePair<string, EntityValue>(nameof(AllowExtraProperties), EntityValue.CreateFromObject(AllowExtraProperties)),
                new KeyValuePair<string, EntityValue>(nameof(Properties), EntityValue.CreateFromObject(propertiesEntity)),
            };


            var entity = new Entity(topProperties);

            return entity;
        }
    }
}
