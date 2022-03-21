using System.IO;
using Reductech.Sequence.Core.Enums;
using RestSharp;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Makes a Http Request to download data from the web.
/// Returns a StringStream containing binary encoded data.
/// </summary>
[Alias("Download")]
[Alias("GetWebStream")]
public class HttpRequest : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stuff = await stateMonad.RunStepsAsync(
            Uri.WrapStringStream(),
            Headers.WrapNullable(),
            cancellationToken
        );

        if (stuff.IsFailure)
            return stuff.ConvertFailure<StringStream>();

        var (uri, headers) = stuff.Value;

        var request = new RestRequest(uri);

        if (headers.HasValue)
            request = AddHeaders(request, headers.GetValueOrThrow());

        var restClient = stateMonad.ExternalContext.RestClientFactory.CreateRestClient(uri);

        Stream? response;

        try
        {
            //TODO do this async
            response = await restClient.DownloadStreamAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return ErrorCode.Unknown.ToErrorBuilder(ex).WithLocationSingle(this);
        }

        var ss = new StringStream(
            response ?? new MemoryStream(Array.Empty<byte>()),
            EncodingEnum.Binary
        );

        return ss;
    }

    /// <summary>
    /// The base url
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("From")]
    [Alias("File")]
    public IStep<StringStream> Uri { get; set; } = null!;

    /// <summary>
    /// Additional headers to send
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("No Additional Headers")]
    public IStep<Entity>? Headers { get; set; }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<HttpRequest, StringStream>();

    static RestRequest AddHeaders(RestRequest request, Entity entity)
    {
        foreach (var (name, sclObject, _) in entity.Dictionary.Values)
        {
            request = request.AddHeader(
                name,
                sclObject.Serialize(SerializeOptions.Primitive)
            );
        }

        return request;
    }
}
