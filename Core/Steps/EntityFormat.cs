namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Formats an entity as a string
/// </summary>
[Alias("Format")]
[AllowConstantFolding]
public sealed class EntityFormat : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entity = await Entity.Run(stateMonad, cancellationToken);

        if (entity.IsFailure)
            return entity.ConvertFailure<StringStream>();

        var result = entity.Value.Format();

        return new StringStream(result);
    }

    /// <summary>
    /// The Entity to Format
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityFormat, StringStream>();
}
