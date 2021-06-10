using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Apply a function to every entity in an entity stream.
/// </summary>
public sealed class EntityMap : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);

        if (entityStreamResult.IsFailure)
            return entityStreamResult.ConvertFailure<Array<Entity>>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async ValueTask<Entity> Action(Entity record)
        {
            await using var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                Variable,
                new KeyValuePair<VariableName, object>(Variable, record)
            );

            var result = await Function.Run(scopedMonad, cancellationToken);

            if (result.IsFailure)
                throw new ErrorException(result.Error);

            return result.Value;
        }

        var newStream = entityStreamResult.Value.SelectAwait(Action);

        return newStream;
    }

    /// <summary>
    /// The entities to map
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> EntityStream { get; set; } = null!;

    /// <summary>
    /// A function to get the mapped entity
    /// </summary>
    [StepProperty(2)]
    [Required]
    [ScopedFunction]
    public IStep<Entity> Function { get; set; } = null!;

    /// <summary>
    /// The variable name to use in the function.
    /// </summary>
    [VariableName(3)]
    [DefaultValueExplanation("<Entity>")]
    public VariableName Variable { get; set; } = VariableName.Entity;

    /// <inheritdoc />
    public override Result<TypeResolver, IError> TryGetScopedTypeResolver(
        TypeResolver baseContext,
        IFreezableStep scopedStep)
    {
        return baseContext.TryCloneWithScopedStep(
            Variable,
            TypeReference.Actual.Entity,
            new CallerMetadata(Name, nameof(Function), TypeReference.Actual.Entity),
            scopedStep,
            new ErrorLocation(this)
        );
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityMap, Array<Entity>>();
}

}
