using Microsoft.Extensions.Logging;
using Reductech.Sequence.Core.Abstractions;
using Reductech.Sequence.Core.Internal.Logging;

namespace Reductech.Sequence.Core;

/// <summary>
/// The state monad that is passed between steps.
/// </summary>
public interface IStateMonad : IAsyncDisposable
{
    /// <summary>
    /// Constant metadata for the entire sequence
    /// </summary>
    IReadOnlyDictionary<string, object> SequenceMetadata { get; }

    /// <summary>
    /// The logger that steps will use to output messages.
    /// </summary>
    ILogger Logger { get; }

    /// <summary>
    /// The external context
    /// </summary>
    IExternalContext ExternalContext { get; }

    /// <summary>
    /// The step factory store. Maps from step names to step factories.
    /// </summary>
    StepFactoryStore StepFactoryStore { get; }

    /// <summary>
    /// Gets all VariableNames and associated objects.
    /// </summary>
    /// <returns></returns>
    IEnumerable<KeyValuePair<VariableName, ISCLObject>> GetState();

    /// <summary>
    /// Gets the current value of this variable.
    /// </summary>
    Result<T, IErrorBuilder> GetVariable<T>(VariableName key) where T : ISCLObject;

    /// <summary>
    /// Returns whether a particular variable has been set and not removed.
    /// </summary>
    bool VariableExists(VariableName variable);

    /// <summary>
    /// Creates or set the value of this variable.
    /// </summary>
    Task<Result<Unit, IError>> SetVariableAsync<T>(
        VariableName key,
        T variable,
        bool disposeOld,
        IStep? callingStep,
        CancellationToken cancellation) where T : ISCLObject;

    /// <summary>
    /// Removes the variable if it exists.
    /// </summary>
    Task RemoveVariableAsync(VariableName key, bool dispose, IStep? callingStep);

    /// <summary>
    /// Logs a message not associated with a situation.
    /// </summary>
    void Log(LogLevel logLevel, string message, IStep? callingStep) =>
        Logger.LogMessage(logLevel, message, callingStep, this);

    /// <summary>
    /// The most recent automatic variable introduced, if there is one.
    /// </summary>
    Maybe<VariableName> AutomaticVariable { get; }

    /// <summary>
    /// A settings Entity containing Connector Settings
    /// </summary>
    Entity Settings { get; }
}
