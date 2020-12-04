using System.Collections.Generic;
using Reductech.EDR.Core.Attributes;

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
        [ConfigProperty(Order = 1)]
        public SchemaPropertyType Type { get; set; }

        /// <summary>
        /// The multiplicity of the property.
        /// </summary>
        [ConfigProperty(Order = 2)]
        public Multiplicity Multiplicity { get; set; } = Multiplicity.Any;

        /// <summary>
        /// The format strings.
        /// For Date, this will contain possible date formats.
        /// For Enum, this will contain possible enum values.
        /// </summary>
        [ConfigProperty(Order = 3)]
        public List<string>? Format { get; set; }

        /// <summary>
        /// A regex to validate the string form of the field value
        /// </summary>
        [ConfigProperty(Order = 4)]
        public string? Regex { get; set; }

        /// <summary>
        /// Convert this SchemaProperty to an entity
        /// </summary>
        /// <returns></returns>
        public object ConvertToEntity()
        {
            var topProperties = new[]
            {
                new KeyValuePair<string, EntityValue>(nameof(Type), EntityValue.CreateFromObject(Type)),
                new KeyValuePair<string, EntityValue>(nameof(Multiplicity), EntityValue.CreateFromObject(Multiplicity)),
                new KeyValuePair<string, EntityValue>(nameof(Format), EntityValue.CreateFromObject(Format)),
                new KeyValuePair<string, EntityValue>(nameof(Regex), EntityValue.CreateFromObject(Regex)),
            };


            var entity = new Entity(topProperties);

            return entity;
        }
    }
}