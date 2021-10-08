using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
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
/// Get JSON from a REST service
/// </summary>
public sealed class RESTGetJSON : CompoundStep<Entity>
{
    /// <inheritdoc />
    protected override async Task<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stuff = await stateMonad.RunStepsAsync(
            URL.WrapStringStream(),
            Headers.WrapNullable(),
            cancellationToken
        );

        if (stuff.IsFailure)
            return stuff.ConvertFailure<Entity>();

        var (url, headers) = stuff.Value;

        IRestRequest request = new RestRequest(url, Method.GET);

        if (headers.HasValue)
            request = request.AddHeaders(headers.GetValueOrThrow());

        var jsonString =
            await request.TryRun(stateMonad.RestClient, cancellationToken);

        if (jsonString.IsFailure)
            return jsonString.ConvertFailure<Entity>().MapError(x => x.WithLocation(this));

        Entity? entity;

        try
        {
            entity = JsonConvert.DeserializeObject<Entity>(
                jsonString.Value,
                EntityJsonConverter.Instance
            )!;
        }
        catch (Exception e)
        {
            stateMonad.Log(LogLevel.Error, e.Message, this);
            entity = null;
        }

        if (entity is null)
            return
                Result.Failure<Entity, IError>(
                    ErrorCode.CouldNotParse.ToErrorBuilder(jsonString, "JSON")
                        .WithLocation(this)
                );

        return entity;
    }

    /// <summary>
    /// The url to get data from
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

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<RESTGetJSON, Entity>();
}

}
