using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Combine two entities.
/// Property values of the second entity will be prioritized.
/// </summary>
public sealed class EntityCombine : BaseOperatorStep<EntityCombine, Entity, Entity>
{
    /// <inheritdoc />
    protected override Result<Entity, IErrorBuilder> Operate(IEnumerable<Entity> terms)
    {
        var r =
            terms.Aggregate((a, b) => a.Combine(b));

        return r;
    }

    /// <inheritdoc />
    public override string Operator => "+";
}

}
