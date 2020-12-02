using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Serialization
{
    ///// <summary>
    ///// Include a variable name in a serialization.
    ///// </summary>
    //public class VariableNameComponent : ISerializerBlock, IStepSerializerComponent
    //{
    //    /// <summary>
    //    /// Deserializes a regex group into a Variable Name.
    //    /// </summary>
    //    public VariableNameComponent(string propertyName) => PropertyName = propertyName;

    //    /// <summary>
    //    /// The name of the property.
    //    /// </summary>
    //    public string PropertyName { get; }
    //    //.Bind(Serialize);

    //    /// <summary>
    //    /// Serialize a variable name.
    //    /// </summary>
    //    public static string Serialize(VariableName vn)


    //    /// <inheritdoc />
    //    public ISerializerBlock SerializerBlock => this;

    //    /// <inheritdoc />
    //    public string GetSegmentText(IReadOnlyDictionary<string, StepProperty> dictionary)
    //    {
    //        dictionary[PropertyName].SerializeValue();


    //        return Serialize();
    //    }
    //}
}