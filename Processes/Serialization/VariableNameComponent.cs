using System;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Include a required space in serialization.
    /// </summary>
    public class SpaceComponent : ICustomSerializerComponent, ISerializerBlock, IDeserializerBlock
    {
        private SpaceComponent() { }

        public static SpaceComponent Instance { get; } = new SpaceComponent();

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        /// <inheritdoc />
        public IDeserializerBlock? DeserializerBlock => this;

        /// <inheritdoc />
        public IDeserializerMapping? Mapping => null;

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableProcessData data) => " ";

        /// <inheritdoc />
        public string GetRegexText(int index) => @"\s+";
    }


    /// <summary>
    /// Include a fixed string in serialization.
    /// </summary>
    public class FixedStringComponent : ICustomSerializerComponent, ISerializerBlock, IDeserializerBlock
    {
        public FixedStringComponent(string value, SpaceType spaces)
        {
            Value = value;
            Spaces = spaces;
        }

        /// <summary>
        /// The fixed value to insert.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// The type of spaces to use.
        /// </summary>
        public SpaceType Spaces { get; }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        /// <inheritdoc />
        public IDeserializerBlock? DeserializerBlock => this;

        /// <inheritdoc />
        public IDeserializerMapping? Mapping => null;

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableProcessData data)
        {
            return Spaces switch
            {
                SpaceType.None => Value,
                SpaceType.Required => $" {Value} ",
                SpaceType.Optional => $" {Value} ",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <inheritdoc />
        public string GetRegexText(int index)
        {
            return Spaces switch
            {
                SpaceType.None => Regex.Escape(Value),
                SpaceType.Required => $@"\s+{Regex.Escape(Value)}\s+",
                SpaceType.Optional => $@"\s*{Regex.Escape(Value)}\s*",
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        public enum SpaceType
        {
            None,
            Required,
            Optional
        }
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