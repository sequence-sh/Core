using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Include a required space in serialization.
    /// </summary>
    public class SpaceComponent : ICustomSerializerComponent, ISerializerBlock, IDeserializerBlock
    {
        /// <summary>
        /// Create a new Space Component.
        /// </summary>
        /// <param name="required"></param>
        public SpaceComponent(bool required) => Required = required;

        /// <summary>
        /// Whether this space is required.
        /// </summary>
        public bool Required { get; }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        /// <inheritdoc />
        public IDeserializerBlock? DeserializerBlock => this;

        /// <inheritdoc />
        public IDeserializerMapping? Mapping => null;

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableProcessData data) => " ";

        /// <inheritdoc />
        public string GetRegexText(int index) => Required? @"\s+" : @"\s*";
    }


    /// <summary>
    /// Include a fixed string in serialization.
    /// </summary>
    public class FixedStringComponent : ICustomSerializerComponent, ISerializerBlock, IDeserializerBlock
    {
        public FixedStringComponent(string value) => Value = value;

        /// <summary>
        /// The fixed value to insert.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        /// <inheritdoc />
        public IDeserializerBlock? DeserializerBlock => this;

        /// <inheritdoc />
        public IDeserializerMapping? Mapping => null;

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableProcessData data) => Value;

        /// <inheritdoc />
        public string GetRegexText(int index) => Regex.Escape(Value);
    }


    /// <summary>
    /// Include a variable name in a serialization.
    /// </summary>
    public class VariableNameComponent : IDeserializerMapping, ISerializerBlock, IDeserializerBlock, ICustomSerializerComponent
    {
        /// <summary>
        /// Deserializes a regex group into a Variable Name.
        /// </summary>
        public VariableNameComponent( string propertyName) => PropertyName = propertyName;


        /// <inheritdoc />
        public string GetGroupName(int index) => "VariableName" + index;

        /// <inheritdoc />
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore) => new ProcessMember(new VariableName(groupText));

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableProcessData data) =>
            data.GetVariableName(PropertyName)
                .Bind(Serialize);

        /// <summary>
        /// Serialize a variable name.
        /// </summary>
        public static Result<string> Serialize(VariableName vn) => $"<{vn.Name}>";

        /// <inheritdoc />
        public string GetRegexText(int index) => $@"<(?<{GetGroupName(index)}>[_\.\w\d]+)>";

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        /// <inheritdoc />
        public IDeserializerBlock? DeserializerBlock => this;

        /// <inheritdoc />
        public IDeserializerMapping? Mapping => this;
    }
}