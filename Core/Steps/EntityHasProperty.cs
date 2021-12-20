namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Checks if an entity has a particular property.
/// </summary>
[Alias("DoesEntityHave")]
[Alias("DoesEntity")]
[SCLExample("DoesEntity ('type': 'C', 'value': 1) Have: 'type'",  "True")]
[SCLExample("DoesEntity ('type': null, 'value': 1) Have: 'type'", "True")]
[SCLExample("EntityHasProperty ('type': 'C', 'value': 1) 'name'", "False")]
public sealed class EntityHasProperty : CompoundStep<SCLBool>
{
    /// <inheritdoc />
    protected override async Task<Result<SCLBool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(
            Entity,
            Property.WrapStringStream(),
            cancellationToken
        );

        if (r.IsFailure)
            return r.ConvertFailure<SCLBool>();

        var (entity, property) = r.Value;

        var result = entity.TryGetValue(property).HasValue;

        return result.ConvertToSCLObject();
    }

    /// <summary>
    /// The entity to check the property on.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <summary>
    /// The name of the property to check.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("Have")]
    public IStep<StringStream> Property { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityHasProperty, SCLBool>();
}
