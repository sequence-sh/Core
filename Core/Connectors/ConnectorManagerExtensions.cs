using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.ConnectorManagement.Base;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Connectors
{

/// <summary>
/// Contains extension methods for working with connector managers.
/// </summary>
public static class ConnectorManagerExtensions
{
    /// <summary>
    /// Creates an external context from a connector manager
    /// </summary>
    public static async Task<Result<IExternalContext, IErrorBuilder>> GetExternalContextAsync(
        this IConnectorManager connectorManager,
        IExternalProcessRunner externalProcessRunner,
        IRestClientFactory restClientFactory,
        IConsole console,
        CancellationToken ct = default,
        params ConnectorData[] additionalConnectors)
    {
        if (!await connectorManager.Verify(ct))
            return ErrorCode.CouldNotCreateStepFactoryStore.ToErrorBuilder(
                "Could not validate installed connectors."
            );

        var connectors = connectorManager.List()
            .Select(c => c.data)
            .Where(c => c.ConnectorSettings.Enable)
            .ToArray();

        var injectedContextsResult =
            connectors
                .SelectMany(x => x.GetConnectorInjections())
                .Select(x => x.TryGetInjectedContexts())
                .Combine(ErrorBuilderList.Combine)
                .Map(x => x.SelectMany(y => y).ToArray());

        if (injectedContextsResult.IsFailure)
            return injectedContextsResult.ConvertFailure<IExternalContext>();

        var context = new ExternalContext(
            externalProcessRunner,
            restClientFactory,
            console,
            injectedContextsResult.Value
        );

        return context;
    }

    /// <summary>
    /// Gets a StepFactory store from a connector manager
    /// </summary>
    public static async Task<Result<StepFactoryStore, IErrorBuilder>> GetStepFactoryStoreAsync(
        this IConnectorManager connectorManager,
        IExternalContext externalContext,
        CancellationToken ct = default,
        params ConnectorData[] additionalConnectors)
    {
        if (!await connectorManager.Verify(ct))
            return ErrorCode.CouldNotCreateStepFactoryStore.ToErrorBuilder(
                "Could not validate installed connectors."
            );

        var connectors = connectorManager.List()
            .Select(c => c.data)
            .Where(c => c.ConnectorSettings.Enable)
            .Concat(additionalConnectors)
            .ToArray();

        if (connectors.GroupBy(c => c.ConnectorSettings.Id).Any(g => g.Count() > 1))
            return ErrorCode.CouldNotCreateStepFactoryStore.ToErrorBuilder(
                "When using multiple configurations with the same connector id, at most one can be enabled."
            );

        var stepFactoryStore =
            StepFactoryStore.TryCreate(
                externalContext,
                connectors
            );

        return stepFactoryStore;
    }

    /// <summary>
    /// Represents errors that occur when configuring or validating connectors.
    /// </summary>
    public class ConnectorConfigurationException : Exception
    {
        /// <inheritdoc />
        public ConnectorConfigurationException(string message) : base(message) { }
    }
}

}
