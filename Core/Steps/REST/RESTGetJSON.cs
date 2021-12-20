namespace Reductech.Sequence.Core.Steps.REST;

/// <summary>
/// Get JSON from a REST service
/// </summary>
public sealed class RESTGetJSON : RESTStep<Entity>
{
    /// <inheritdoc />
    public override Method Method => Method.GET;

    /// <inheritdoc />
    protected override async Task<Result<IRestRequest, IError>> SetRequestBody(
        IStateMonad stateMonad,
        IRestRequest restRequest,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result.Success<IRestRequest, IError>(restRequest);
    }

    /// <inheritdoc />
    protected override Result<Entity, IErrorBuilder> GetResult(string s)
    {
        return TryDeserializeToEntity(s);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<RESTGetJSON, Entity>();
}
