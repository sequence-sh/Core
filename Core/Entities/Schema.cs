using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
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
}
