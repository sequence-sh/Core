using System.Collections.Generic;
using Reductech.EDR.ConnectorManagement.Base;
using Reductech.EDR.Core.Abstractions;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Generates step factories dynamically based on configuration.
/// </summary>
public interface IDynamicStepGenerator
{
    /// <summary>
    /// Creates step factories.
    /// </summary>
    IEnumerable<IStepFactory> CreateStepFactories(
        ConnectorSettings connectorSettings,
        IExternalContext externalContext);
}

}
