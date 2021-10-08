using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Flurl.Http;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps.REST
{

/// <summary>
/// Get data from a REST service
/// </summary>
public sealed class RESTGetStream : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stuff = await stateMonad.RunStepsAsync(
            URL.WrapStringStream(),
            Headers.WrapNullable(),
            cancellationToken
        );

        if (stuff.IsFailure)
            return stuff.ConvertFailure<StringStream>();

        var (url, headers) = stuff.Value;

        var request = url.WithClient(stateMonad.FlurlClient);

        if (headers.HasValue)
            request = request.AddHeaders(headers.GetValueOrThrow());

        var stream = await request.TryRun(x => x.GetStreamAsync(cancellationToken));

        if (stream.IsFailure)
            return stream.ConvertFailure<StringStream>().MapError(x => x.WithLocation(this));

        var result = new StringStream(stream.Value, EncodingEnum.UTF8);
        return result;
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
        new SimpleStepFactory<RESTGetStream, StringStream>();
}

}
