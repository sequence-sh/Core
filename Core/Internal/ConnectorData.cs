using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Connectors;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Information about a connector
/// </summary>
public record ConnectorData(
    ConnectorSettings ConnectorSettings,
    Assembly? Assembly)
{
    /// <summary>
    /// Get injections for this connector
    /// </summary>
    public IReadOnlyCollection<IConnectorInjection> GetConnectorInjections() => Assembly?.GetTypes()
        .Where(x => !x.IsAbstract)
        .Where(x => typeof(IConnectorInjection).IsAssignableFrom(x))
        .Select(Activator.CreateInstance)
        .Cast<IConnectorInjection>()
        .ToArray() ?? ArraySegment<IConnectorInjection>.Empty;

    /// <summary>
    /// Tries to get contexts injected by connectors
    /// </summary>
    public Result<(string Name, object Context)[], IErrorBuilder> TryGetInjectedContexts(
        SCLSettings settings)
    {
        var contexts = GetConnectorInjections()
            .Select(x => x.TryGetInjectedContexts(settings))
            .Combine(x => x.SelectMany(y => y).ToArray(), ErrorBuilderList.Combine);

        return contexts;
    }

    /// <inheritdoc />
    public override string ToString() => ConnectorSettings.ToString()!;
}

}
