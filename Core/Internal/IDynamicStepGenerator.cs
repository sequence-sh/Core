using Sequence.Core.Abstractions;

namespace Sequence.Core.Internal;

/// <summary>
/// Generates step factories dynamically based on configuration.
/// </summary>
public interface IDynamicStepGenerator
{
    /// <summary>
    /// Creates step factories.
    /// </summary>
    Result<IReadOnlyList<IStepFactory>, IErrorBuilder> TryCreateStepFactories(
        ConnectorSettings connectorSettings,
        IExternalContext externalContext);
}
