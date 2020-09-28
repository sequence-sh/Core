using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Include a required space in serialization.
    /// </summary>
    public class SpaceComponent : IProcessSerializerComponent, ISerializerBlock//, IDeserializerBlock
    {
        /// <summary>
        /// Create a new Space Component.
        /// </summary>
        public SpaceComponent()
        {

        }


        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableProcessData data) => " ";
    }


    /// <summary>
    /// Include a fixed string in serialization.
    /// </summary>
    public class FixedStringComponent : IProcessSerializerComponent, ISerializerBlock//, IDeserializerBlock
    {
        /// <summary>
        /// Creates a new FixedStringComponent
        /// </summary>
        public FixedStringComponent(string value) => Value = value;

        /// <summary>
        /// The fixed value to insert.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableProcessData data) => Value;
    }


    /// <summary>
    /// Include a variable name in a serialization.
    /// </summary>
    public class VariableNameComponent : ISerializerBlock, IProcessSerializerComponent
    {
        /// <summary>
        /// Deserializes a regex group into a Variable Name.
        /// </summary>
        public VariableNameComponent(string propertyName) => PropertyName = propertyName;

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableProcessData data) =>
            data.GetVariableName(PropertyName)
                .Bind(Serialize);

        /// <summary>
        /// Serialize a variable name.
        /// </summary>
        public static Result<string> Serialize(VariableName vn) => $"<{vn.Name}>";


        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;
    }
}