namespace Reductech.Sequence.Core.Connectors;

/// <summary>
/// Provides Core-specific extension methods for ConnectorData
/// </summary>
public static class ConnectorDataExtensions
{
    /// <summary>
    /// Get injections for this connector
    /// </summary>
    public static IReadOnlyCollection<IConnectorInjection> GetConnectorInjections(
        this ConnectorData connectorData) => connectorData.Assembly?.GetTypes()
        .Where(x => !x.IsAbstract)
        .Where(x => typeof(IConnectorInjection).IsAssignableFrom(x))
        .Select(Activator.CreateInstance)
        .Cast<IConnectorInjection>()
        .ToArray() ?? ArraySegment<IConnectorInjection>.Empty;

    /// <summary>
    /// Tries to get contexts injected by connectors
    /// </summary>
    public static Result<(string Name, object Context)[], IErrorBuilder> TryGetInjectedContexts(
        this ConnectorData connectorData)
    {
        var contexts = connectorData.GetConnectorInjections()
            .Select(x => x.TryGetInjectedContexts())
            .Combine(x => x.SelectMany(y => y).ToArray(), ErrorBuilderList.Combine);

        return contexts;
    }
}
