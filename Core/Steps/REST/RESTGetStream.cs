namespace Reductech.Sequence.Core.Steps.REST;

/// <summary>
/// Get data from a REST service
/// </summary>
public sealed class RESTGetStream : RESTStep<StringStream>
{
    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<RESTGetStream, StringStream>();

    /// <inheritdoc />
    public override Method Method => Method.Get;

    /// <inheritdoc />
    protected override async Task<Result<RestRequest, IError>> SetRequestBody(
        IStateMonad stateMonad,
        RestRequest restRequest,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result.Success<RestRequest, IError>(restRequest);
    }

    /// <inheritdoc />
    protected override Result<StringStream, IErrorBuilder> GetResult(string s)
    {
        return new StringStream(s);
    }
}
