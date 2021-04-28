using System;
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
public record ConnectorInformation(
    string Name,
    string Version,
    Entity Settings, //Special settings for this connector
    IConnectorInjection[] ConnectorInjections)
{
    /// <summary>
    /// Create from an assembly
    /// </summary>
    public static ConnectorInformation? TryCreate(Assembly? assembly, ConnectorSettings settings)
    {
        if (assembly is null)
            return null;

        if (!settings.Enable)
            return null;

        try
        {
            //FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            //string          version         = fileVersionInfo.ProductVersion!;
            var connectorInjections = assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(IConnectorInjection).IsAssignableFrom(x))
                .Select(Activator.CreateInstance)
                .Cast<IConnectorInjection>()
                .ToArray();

            return new ConnectorInformation(
                settings.Id,
                settings.Version,
                settings.Settings,
                connectorInjections
            );
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Tries to get contexts injected by connectors
    /// </summary>
    public Result<(string Name, object Context)[], IErrorBuilder> TryGetInjectedContexts(
        SCLSettings settings)
    {
        var contexts = ConnectorInjections.Select(x => x.TryGetInjectedContexts(settings))
            .Combine(x => x.SelectMany(y => y).ToArray(), ErrorBuilderList.Combine);

        return contexts;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name} {Version}";
    }
}

}
