using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Include a variable name in a serialization.
    /// </summary>
    public class VariableNameComponent : ISerializerBlock, IStepSerializerComponent
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
        public Result<string> TryGetText(FreezableStepData data) =>
            data.GetVariableName(PropertyName, "Type").MapError(x=>x.AsString)
                .Bind(Serialize);

        /// <summary>
        /// Serialize a variable name.
        /// </summary>
        public static Result<string> Serialize(VariableName vn) => $"<{vn.Name}>";


        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;
    }
}