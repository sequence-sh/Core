namespace Reductech.Sequence.Core.Steps.REST;

/// <summary>
/// Executes a REST Patch request
/// </summary>
public sealed class RESTPatch : RESTStep<Unit>
{
    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<RESTPatch, Unit>();

    /// <inheritdoc />
    public override Method Method => Method.Patch;

    /// <inheritdoc />
    protected override Task<Result<RestRequest, IError>> SetRequestBody(
        IStateMonad stateMonad,
        RestRequest restRequest,
        CancellationToken cancellationToken)
    {
        return SetRequestJSONBody(stateMonad, restRequest, Entity, cancellationToken);
    }

    /// <inheritdoc />
    protected override Result<Unit, IErrorBuilder> GetResult(string s)
    {
        return Unit.Default;
    }

    /// <summary>
    /// The Entity to create
    /// </summary>
    [StepProperty(3)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;
}
