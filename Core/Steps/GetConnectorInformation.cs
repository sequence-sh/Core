using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets information about connectors
/// </summary>
public sealed class GetConnectorInformation : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var entities = new List<Entity>();

        foreach (var (connectorSettings, _) in stateMonad.StepFactoryStore.ConnectorData)
        {
            var entity = Entity.Create(
                (nameof(ConnectorSettings.Id), connectorSettings.Id),
                (nameof(ConnectorSettings.Version), connectorSettings.Version)
            );

            entities.Add(entity);
        }

        return entities.ToSCLArray();
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<GetConnectorInformation, Array<Entity>>();
}

}
