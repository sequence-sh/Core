using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using RestSharp;

namespace Reductech.EDR.Core.Steps.REST
{

/// <summary>
/// A step that calls a rest service
/// </summary>
/// <typeparam name="TOutput"></typeparam>
public abstract class RESTStep<TOutput> : CompoundStep<TOutput>
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
            URL.WrapStringStream(),
            Headers.WrapNullable(),
            cancellationToken
        );

        if (stuff.IsFailure)
            return stuff.ConvertFailure<TOutput>();

        var (url, headers) = stuff.Value;

        IRestRequest request = new RestRequest(url, Method);

        if (headers.HasValue)
            request = request.AddHeaders(headers.GetValueOrThrow());

        var setBodyResult = await SetRequestBody(stateMonad, request, cancellationToken);

        if (setBodyResult.IsFailure)
            return setBodyResult.ConvertFailure<TOutput>();

        request = setBodyResult.Value;

        var resultString =
            await request.TryRun(stateMonad.RestClient, cancellationToken);

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
    /// The url to send the request to
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> URL { get; set; } = null!;

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

        var serializedObject = JsonConvert.SerializeObject(r.Value, EntityJsonConverter.Instance);

        restRequest = restRequest.AddJsonBody(serializedObject);

        return Result.Success<IRestRequest, IError>(restRequest);
    }

    /// <summary>
    /// Try deserialize a string to an entity
    /// </summary>
    protected static Result<Entity, IErrorBuilder> TryDeserializeToEntity(string jsonString)
    {
        Entity? entity;

        try
        {
            entity = JsonConvert.DeserializeObject<Entity>(
                jsonString,
                EntityJsonConverter.Instance
            );
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

}
