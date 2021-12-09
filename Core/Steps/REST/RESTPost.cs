using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using RestSharp;

namespace Reductech.EDR.Core.Steps.REST;

/// <summary>
/// Create a REST resource and return the id of the created resource
/// </summary>
public sealed class RESTPost : RESTStep<StringStream>
{
    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<RESTPost, StringStream>();

    /// <inheritdoc />
    public override Method Method => Method.POST;

    /// <inheritdoc />
    protected override Task<Result<IRestRequest, IError>> SetRequestBody(
        IStateMonad stateMonad,
        IRestRequest restRequest,
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
