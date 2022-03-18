using System.IO;
using Reductech.Sequence.Core.Enums;

namespace Reductech.Sequence.Core.Steps.REST;

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

        IRestRequest request = new RestRequest(uri, Method.GET);

        if (headers.HasValue)
            request = request.AddHeaders(headers.GetValueOrThrow());

        var restClient = stateMonad.ExternalContext.RestClientFactory.CreateRestClient(uri);

        byte[] response;

        try
        {
            //TODO do this async
            response = restClient.DownloadData(request);
        }
        catch (Exception ex)
        {
            return ErrorCode.Unknown.ToErrorBuilder(ex).WithLocationSingle(this);
        }

        var ss = new StringStream(new MemoryStream(response), EncodingEnum.Binary);
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
}
