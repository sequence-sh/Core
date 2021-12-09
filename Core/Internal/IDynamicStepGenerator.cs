using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.ConnectorManagement.Base;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal;

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
