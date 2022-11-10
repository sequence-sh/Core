namespace Sequence.Core.Steps;

/// <summary>
/// Formats an entity or an array of entities as a string
/// </summary>
[Alias("Format")]
[AllowConstantFolding]
[SCLExample("EntityFormat (a:1, b:2)",               "( 'a': 1 'b': 2 )")]
[SCLExample("EntityFormat [(a:1, b:2), (a:3, b:4)]", "[ ( 'a': 1 'b': 2 ), ( 'a': 3 'b': 4 ) ]")]
public sealed class EntityFormat : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var oneOf = await Entity.Run(stateMonad, cancellationToken);

        if (oneOf.IsFailure)
            return oneOf.ConvertFailure<StringStream>();

        var sb = new IndentationStringBuilder();

        var serializable = oneOf.Value.OneOf.Match(x => x, x => x as ISerializable);

        serializable.Format(
            sb,
            new FormattingOptions(),
            new Stack<Comment>()
        );

        return new StringStream(sb.ToString());
    }

    /// <summary>
    /// The Entity to Format
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<SCLOneOf<Entity, Array<Entity>>> Entity { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityFormat, StringStream>();
}
