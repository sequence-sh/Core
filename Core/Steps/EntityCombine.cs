namespace Sequence.Core.Steps;

/// <summary>
/// Combine two entities.
/// Property values of the second entity will be prioritized.
/// </summary>
[AllowConstantFolding]
public sealed class EntityCombine : BaseOperatorStep<EntityCombine, Entity, Entity>
{
    /// <inheritdoc />
    protected override Result<Entity, IErrorBuilder> Operate(IEnumerable<Entity> terms) =>
        terms.Aggregate((a, b) => a.Combine(b));

    /// <inheritdoc />
    public override string Operator => "+";
}
