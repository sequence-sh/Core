using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using RestSharp;

namespace Reductech.EDR.Core.Steps.REST
{

/// <summary>
/// Executes a REST Put request
/// </summary>
public sealed class RESTPut : RESTStep<Unit>
{
    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<RESTPut, Unit>();

    /// <inheritdoc />
    public override Method Method => Method.PUT;

    /// <inheritdoc />
    protected override Task<Result<IRestRequest, IError>> SetRequestBody(
        IStateMonad stateMonad,
        IRestRequest restRequest,
        CancellationToken cancellationToken)
    {
        return SetRequestJSONBody(stateMonad, restRequest, Entity, cancellationToken);
    }

    /// <inheritdoc />
    protected override Result<Unit, IErrorBuilder> GetResult(string s)
    {
        return Unit.Default;
    }

    /// <summary>
    /// The Entity to create
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;
}

}
