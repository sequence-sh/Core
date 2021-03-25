using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A step that creates and returns an entity.
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public record CreateEntityStep
    (IReadOnlyDictionary<EntityPropertyKey, IStep> Properties) : IStep<Entity>
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <inheritdoc />
    public async Task<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pairs = new List<(EntityPropertyKey, object?)>();

        foreach (var (key, step) in Properties)
        {
            var r = await step.Run<object>(stateMonad, cancellationToken)
                .Bind(x => EntityHelper.TryUnpackObjectAsync(x, cancellationToken));

            if (r.IsFailure)
                return r.ConvertFailure<Entity>();

            pairs.Add((key, r.Value));
        }

        return Entity.Create(pairs);
    }

    /// <inheritdoc />
    public Maybe<EntityValue> TryConvertToEntityValue()
    {
        var pairs = new List<(EntityPropertyKey, object?)>();

        foreach (var (key, value) in Properties)
        {
            var ev = value.TryConvertToEntityValue();

            if (ev.HasNoValue)
                return Maybe<EntityValue>.None;

            pairs.Add((key, ev.Value));
        }

        var entity = Entity.Create(pairs);
        return new EntityValue(entity);
    }

    /// <inheritdoc />
    public string Name => "Create Entity";

    /// <inheritdoc />
    public IFreezableStep Unfreeze()
    {
        var dictionary =
            Properties.ToDictionary(
                x => x.Key,
                x =>
                    new FreezableStepProperty(x.Value.Unfreeze(), TextLocation)
            );

        return new CreateEntityFreezableStep(new FreezableEntityData(dictionary, TextLocation));
    }

    /// <inheritdoc />
    public async Task<Result<T, IError>> Run<T>(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return await Run(stateMonad, cancellationToken)
            .BindCast<Entity, T, IError>(
                ErrorCode.InvalidCast.ToErrorBuilder(
                        Name,
                        typeof(T).Name
                    )
                    .WithLocation(this)
            );
    }

    /// <inheritdoc />
    public Result<Unit, IError> Verify(SCLSettings settings)
    {
        var r = Properties.Select(x => x.Value.Verify(settings))
            .Combine(_ => Unit.Default, ErrorList.Combine);

        return r;
    }

    /// <inheritdoc />
    public TextLocation? TextLocation { get; set; }

    /// <inheritdoc />
    public Type OutputType => typeof(Entity);

    /// <inheritdoc />
    public string Serialize()
    {
        var sb = new StringBuilder();

        sb.Append('(');

        var results = new List<string>();

        foreach (var (key, value) in Properties)
        {
            var valueString = value.Serialize();

            results.Add($"{key}: {valueString}");
        }

        sb.AppendJoin(",", results);

        sb.Append(')');

        return sb.ToString();
    }

    /// <inheritdoc />
    public IEnumerable<Requirement> RuntimeRequirements
    {
        get
        {
            return Properties.SelectMany(x => x.Value.RuntimeRequirements);
        }
    }

    /// <inheritdoc />
    public bool ShouldBracketWhenSerialized => false;
}

}
