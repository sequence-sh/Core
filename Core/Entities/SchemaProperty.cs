using System.Collections.Generic;
using YamlDotNet.Serialization;

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
}