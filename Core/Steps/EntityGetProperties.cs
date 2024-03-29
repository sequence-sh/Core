﻿namespace Sequence.Core.Steps;

/// <summary>
/// Gets the names of all properties of an entity
/// </summary>
[SCLExample(
    "EntityGetProperties (property1: 1, property2: 2)",
    ExpectedOutput = "[\"property1\", \"property2\"]"
)]
[Alias("GetProps")]
[Alias("GetProperties")]
[AllowConstantFolding]
public sealed class EntityGetProperties : CompoundStep<Array<StringStream>>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Array<StringStream>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entity = await Entity.Run(stateMonad, cancellationToken);

        if (entity.IsFailure)
            return entity.ConvertFailure<Array<StringStream>>();

        var array = entity.Value.Headers.Select(x => new StringStream(x.Inner)).ToSCLArray();

        return array;
    }

    /// <summary>
    /// The entity to get properties from
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityGetProperties, Array<StringStream>>();
}
