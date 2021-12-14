using System.Text;

namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Formats an entity as a string
/// </summary>
[Alias("Format")]
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

        var sb = new StringBuilder();
        ((ISCLObject)entity.Value).Format(sb, 0, new FormattingOptions(), null, null);

        return new StringStream(sb.ToString());
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
