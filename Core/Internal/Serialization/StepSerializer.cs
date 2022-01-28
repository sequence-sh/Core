//namespace Reductech.Sequence.Core.Internal.Serialization;

///// <summary>
///// A custom step serializer.
///// </summary>
//public class StepSerializer : IStepSerializer
//{
//    /// <summary>
//    /// Create a new StepSerializer
//    /// </summary>
//    public StepSerializer(string typeName, params ISerializerBlock[] components)
//    {
//        TypeName = typeName;
//        Blocks   = components;
//    }

//    /// <summary>
//    /// The name of the step to serialize
//    /// </summary>
//    public string TypeName { get; }

//    /// <inheritdoc />
//    public override string ToString() => TypeName;

//    /// <summary>
//    /// The component to use.
//    /// </summary>
//    public IReadOnlyCollection<ISerializerBlock> Blocks { get; }

//    /// <inheritdoc />
//    public string Serialize(SerializeOptions options, IEnumerable<StepProperty> stepProperties)
//    {
//        var dict = stepProperties.ToDictionary(x => x.Name);

//        var result = Blocks
//            .Select(x => x.TryGetSegmentText(options, dict))
//            .Combine();

//        if (result.IsSuccess) //The custom serialization worked
//            return string.Join("", result.Value);

//        var defaultSerializer = new FunctionSerializer(TypeName);

//        var r = defaultSerializer.Serialize(options, dict.Values);

//        return r;
//    }
//}


