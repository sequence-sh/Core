namespace Reductech.Sequence.Core.Steps.REST;

/// <summary>
/// Create a REST resource and return the id of the created resource
/// </summary>
public sealed class RESTPost : RESTStep<StringStream>
{
    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<RESTPost, StringStream>();

    /// <inheritdoc />
    public override Method Method => Method.Post;

    /// <inheritdoc />
    protected override Task<Result<RestRequest, IError>> SetRequestBody(
        IStateMonad stateMonad,
        RestRequest restRequest,
        CancellationToken cancellationToken)
    {
        return SetRequestJSONBody(stateMonad, restRequest, Entity, cancellationToken);
    }

    /// <summary>
    /// The Entity to create
    /// </summary>
    [StepProperty(3)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <inheritdoc />
    protected override Result<StringStream, IErrorBuilder> GetResult(string s)
    {
        return new StringStream(s);
    }
}
