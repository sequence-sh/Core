namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// Data for a node
/// </summary>
public interface INodeData
{
    /// <summary>
    /// Set the schema builder with this node data
    /// </summary>
    /// <param name="builder"></param>
    void SetBuilder(JsonSchemaBuilder builder);
}

/// <summary>
/// Contains additional data about a node
/// </summary>
public abstract record NodeData<T> : INodeData where T : NodeData<T>
{
    /// <summary>
    /// Try to combine with another NodeData of the same type
    /// </summary>
    [Pure]
    public abstract T Combine(T other);

    /// <summary>
    /// Set a JsonSchemaBuilder with this data
    /// </summary>
    public abstract void SetBuilder(JsonSchemaBuilder builder);
}
