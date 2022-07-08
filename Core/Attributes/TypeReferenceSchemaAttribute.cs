namespace Reductech.Sequence.Core.Attributes;

/// <summary>
/// Indicates that the return type of this step will have a particular schema
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
public sealed class TypeReferenceSchemaAttribute : Attribute
{
    /// <inheritdoc />
    public TypeReferenceSchemaAttribute(string s)
    {
        var jsonSchema = JsonSchema.FromText(s);
        var schemaNode = SchemaNode.Create(jsonSchema);

        var typeReference = schemaNode.ToTypeReference();

        TypeReference = typeReference.Value;
    }

    /// <summary>
    /// The schema node
    /// </summary>
    public TypeReference TypeReference { get; }
}
