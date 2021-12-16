namespace Reductech.EDR.Core.Steps.REST;

/// <summary>
/// A step that calls a rest service
/// </summary>
public abstract class RESTStep<TOutput> : CompoundStep<TOutput> where TOutput : ISCLObject
{
    /// <summary>
    /// The REST method
    /// </summary>
    public abstract Method Method { get; }

    /// <inheritdoc />
    protected override async Task<Result<TOutput, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stuff = await stateMonad.RunStepsAsync(
            BaseURL.WrapStringStream(),
            RelativeURL.WrapStringStream(),
            Headers.WrapNullable(),
            cancellationToken
        );

        if (stuff.IsFailure)
            return stuff.ConvertFailure<TOutput>();

        var (baseUrl, relativeUrl, headers) = stuff.Value;

        IRestRequest request = new RestRequest(relativeUrl, Method);

        if (headers.HasValue)
            request = request.AddHeaders(headers.GetValueOrThrow());

        var setBodyResult = await SetRequestBody(stateMonad, request, cancellationToken);

        if (setBodyResult.IsFailure)
            return setBodyResult.ConvertFailure<TOutput>();

        request = setBodyResult.Value;

        var restClient = stateMonad.ExternalContext.RestClientFactory.CreateRestClient(baseUrl);

        var resultString =
            await request.TryRun(restClient, cancellationToken);

        if (resultString.IsFailure)
            return resultString.ConvertFailure<TOutput>().MapError(x => x.WithLocation(this));

        var result = GetResult(resultString.Value).MapError(x => x.WithLocation(this));

        return result;
    }

    /// <summary>
    /// Sets the Request Body
    /// </summary>
    protected abstract Task<Result<IRestRequest, IError>> SetRequestBody(
        IStateMonad stateMonad,
        IRestRequest restRequest,
        CancellationToken cancellationToken);

    /// <summary>
    /// Create the result from the output string
    /// </summary>
    protected abstract Result<TOutput, IErrorBuilder> GetResult(string s);

    /// <summary>
    /// The base url
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> BaseURL { get; set; } = null!;

    /// <summary>
    /// The relative url
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<StringStream> RelativeURL { get; set; } = null!;

    /// <summary>
    /// Additional headers to send
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("No Additional Headers")]
    public IStep<Entity>? Headers { get; set; }

    /// <summary>
    /// Sets the JSON body of the REST request
    /// </summary>
    protected static async Task<Result<IRestRequest, IError>> SetRequestJSONBody(
        IStateMonad stateMonad,
        IRestRequest restRequest,
        IStep<Entity> entityStep,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(entityStep, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<IRestRequest>();

        var serializedObject = JsonSerializer
            .Serialize(r.Value);

        restRequest = restRequest.AddJsonBody(serializedObject);

        return Result.Success<IRestRequest, IError>(restRequest);
    }

    /// <summary>
    /// Try deserialize a string to an entity
    /// </summary>
    public static Result<Entity, IErrorBuilder> TryDeserializeToEntity(string jsonString)
    {
        Entity? entity;

        try
        {
            entity = JsonSerializer.Deserialize<Entity>(jsonString);
        }
        catch (Exception e)
        {
            return ErrorCode.Unknown.ToErrorBuilder(e.Message);
        }

        if (entity is null)
            return ErrorCode.CouldNotParse.ToErrorBuilder(jsonString, "JSON");

        return entity;
    }
}
