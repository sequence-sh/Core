//namespace Reductech.Sequence.Core.Entities.Schema;

///// <summary>
///// Matches any of several given values
///// </summary>
//public record OneOfNode(OneOfNodeData Data1) : SchemaNode<OneOfNodeData>(
//    EnumeratedValuesNodeData.Empty,
//    Data1
//)
//{
//    /// <inheritdoc />
//    public override SchemaValueType SchemaValueType => SchemaValueType.Object;

//    /// <inheritdoc />
//    public override bool IsMorePermissive(SchemaNode other) =>
//        Data1.Options.Any(x => x.IsMorePermissive(other));

//    /// <inheritdoc />
//    protected override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
//        string propertyName,
//        ISCLObject value,
//        TransformSettings transformSettings)
//    {
//        var errors = new List<IErrorBuilder>();

//        foreach (var node in Data1.Options)
//        {
//            var result = node.TryTransform(propertyName, value, transformSettings);

//            if (result.IsSuccess)
//                return result.Value;

//            errors.Add(result.Error);
//        }

//        return Result.Failure<Maybe<ISCLObject>, IErrorBuilder>(ErrorBuilderList.Combine(errors));
//    }
//}

///// <summary>
///// Matches any of several given values
///// </summary>
//public record OneOfNodeData(ImmutableList<SchemaNode> Options) : NodeData<OneOfNodeData>
//{
//    /// <inheritdoc />
//    public override OneOfNodeData Combine(OneOfNodeData other)
//    {
//        return new OneOfNodeData(
//            Options.Concat(other.Options).ToImmutableList()
//        ); //not sure if this is correct - should this be and?
//    }

//    /// <inheritdoc />
//    public override void SetBuilder(JsonSchemaBuilder builder)
//    {
//        builder.OneOf(Options.Select(x => x.ToJsonSchema()));
//    }
//}


