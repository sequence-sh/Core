using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using RestSharp;

namespace Reductech.EDR.Core.Steps.REST
{

/// <summary>
/// Delete a REST resource
/// </summary>
public sealed class RESTDelete : RESTStep<Unit>
{
    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<RESTDelete, Unit>();

    /// <inheritdoc />
    public override Method Method => Method.DELETE;

    /// <inheritdoc />
    protected override async Task<Result<IRestRequest, IError>> SetRequestBody(
        IStateMonad stateMonad,
        IRestRequest restRequest,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result.Success<IRestRequest, IError>(restRequest);
    }

    /// <inheritdoc />
    protected override Result<Unit, IErrorBuilder> GetResult(string s)
    {
        return Unit.Default;
    }
}

}
