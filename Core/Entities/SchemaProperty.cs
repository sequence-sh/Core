using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// A single property in a a schema.
    /// </summary>
    public sealed class SchemaProperty
    {
        /// <summary>
        /// The type of the property.
        /// </summary>
        [ConfigProperty(1)]
        public SchemaPropertyType Type { get; set; }

        /// <summary>
        /// If this is an enum, the name of the enum
        /// </summary>
        [ConfigProperty(2)]
        public string? EnumType { get; set; }

        /// <summary>
        /// The multiplicity of the property.
        /// </summary>
        [ConfigProperty(3)]
        public Multiplicity Multiplicity { get; set; } = Multiplicity.Any;

        /// <summary>
        /// The format strings.
        /// For Date, this will contain possible date formats.
        /// For Enum, this will contain possible enum values.
        /// </summary>
        [ConfigProperty(4)]
        public IReadOnlyList<string>? Format { get; set; }

        /// <summary>
        /// A regex to validate the string form of the field value
        /// </summary>
        [ConfigProperty(5)]
        public string? Regex { get; set; }


        /// <summary>
        /// Tries to create a schema from an entity.
        /// Ignores unexpected properties.
        /// </summary>
        public static Result<SchemaProperty, IErrorBuilder> TryCreateFromEntity(Entity entity)
        {
            var results = new List<Result<Unit, IErrorBuilder>>();
            var schemaProperty = new SchemaProperty();

            results.Add(entity.TrySetEnum<SchemaPropertyType>(nameof(Type), nameof(SchemaProperty), s => schemaProperty.Type = s));
            entity.TrySetString(nameof(EnumType), nameof(SchemaProperty), s => schemaProperty.EnumType = s); //Ignore the result of this


            results.Add(entity.TrySetEnum<Multiplicity>(nameof(Multiplicity), nameof(SchemaProperty), s => schemaProperty.Multiplicity = s));
            entity.TrySetStringList(nameof(Format), nameof(SchemaProperty), s => schemaProperty.Format = s);


            entity.TrySetString(nameof(Regex), nameof(SchemaProperty), s => schemaProperty.Regex = s); //Ignore the result of this


            var r = results.Combine(ErrorBuilderList.Combine)
                .Map(_ => schemaProperty);


            return r;

        }


        /// <summary>
        /// Convert this SchemaProperty to an entity
        /// </summary>
        /// <returns></returns>
        public object ConvertToEntity()
        {
            var topProperties = new[]
            {
                new KeyValuePair<string, EntityValue>(nameof(Type), EntityValue.CreateFromObject(Type)),
                new KeyValuePair<string, EntityValue>(nameof(Type), EntityValue.CreateFromObject(EnumType)),
                new KeyValuePair<string, EntityValue>(nameof(Multiplicity), EntityValue.CreateFromObject(Multiplicity)),
                new KeyValuePair<string, EntityValue>(nameof(Format), EntityValue.CreateFromObject(Format)),
                new KeyValuePair<string, EntityValue>(nameof(Regex), EntityValue.CreateFromObject(Regex)),
            };


            var entity = new Entity(topProperties);

            return entity;
        }
    }
}