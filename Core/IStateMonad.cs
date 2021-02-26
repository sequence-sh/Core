using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Logging;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{

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
    /// The settings for this step.
    /// </summary>
    SCLSettings Settings { get; }

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
    IEnumerable<KeyValuePair<VariableName, object>> GetState();

    /// <summary>
    /// Gets the current value of this variable.
    /// </summary>
    Result<T, IErrorBuilder> GetVariable<T>(VariableName key);

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
        IStep? callingStep);

    /// <summary>
    /// Removes the variable if it exists.
    /// </summary>
    Task RemoveVariableAsync(VariableName key, bool dispose, IStep? callingStep);

    /// <summary>
    /// Logs a message not associated with a situation.
    /// </summary>
    void Log(LogLevel logLevel, string message, IStep? callingStep) =>
        Logger.LogMessage(logLevel, message, callingStep, this);
}

}
