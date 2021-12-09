using CSharpFunctionalExtensions;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Abstractions;

/// <summary>
/// The external context of a sequence.
/// Includes the file system, external processes, and the console
/// </summary>
public interface IExternalContext
{
    /// <summary>
    /// For interacting with external processes
    /// </summary>
    public IExternalProcessRunner ExternalProcessRunner { get; }

    /// <summary>
    /// The REST Client Factory.
    /// Creates REST Clients for interacting with web services.
    /// </summary>
    public IRestClientFactory RestClientFactory { get; }

    /// <summary>
    /// For interacting with the console
    /// </summary>
    public IConsole Console { get; }

    /// <summary>
    /// Get context of a given type.
    /// Allows for type injection.
    /// </summary>
    public Result<T, IErrorBuilder> TryGetContext<T>(string name) where T : class;

    /// <summary>
    /// /// Injected contexts
    /// </summary>
    public (string name, object context)[] InjectedContexts { get; }
}
