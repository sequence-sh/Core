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
}

///// <summary>
///// Combine two entities.
///// Property values of the second entity will be prioritized.
///// </summary>
//public sealed class EntityCombine : CompoundStep<Entity>
//{
//    /// <inheritdoc />
//    protected override async Task<Result<Entity, IError>> Run(
//        IStateMonad stateMonad,
//        CancellationToken cancellationToken)
//    {
//        var first = await First.Run(stateMonad, cancellationToken);

//        if (first.IsFailure)
//            return first.ConvertFailure<Entity>();

//        var second = await Second.Run(stateMonad, cancellationToken);

//        if (second.IsFailure)
//            return second.ConvertFailure<Entity>();

//        var r = first.Value.Combine(second.Value);

//        return r;
//    }

//    /// <summary>
//    /// The first entity.
//    /// </summary>
//    [StepProperty(1)]
//    [Required]
//    public IStep<Entity> First { get; set; } = null!;

//    /// <summary>
//    /// The second entity.
//    /// </summary>
//    [StepProperty(2)]
//    [Required]
//    public IStep<Entity> Second { get; set; } = null!;

//    /// <inheritdoc />
//    public override IStepFactory StepFactory => EntityCombineStepFactory.Instance;
//}

///// <summary>
///// Combine two entities.
///// Property values of the second entity will be prioritized.
///// </summary>
//public sealed class EntityCombineStepFactory : SimpleStepFactory<EntityCombine, Entity>
//{
//    private EntityCombineStepFactory() { }

//    /// <summary>
//    /// The instance
//    /// </summary>
//    public static SimpleStepFactory<EntityCombine, Entity> Instance { get; } =
//        new EntityCombineStepFactory();

//    /// <inheritdoc />
//    public override IStepSerializer Serializer => EntityCombineSerializer.Instance;
//}

}
