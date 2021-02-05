using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Thinktecture;
using Thinktecture.Adapters;

namespace Reductech.EDR.Core
{

/// <summary>
/// The external context of a sequence.
/// Includes the file system, external processes, and the console
/// </summary>
public interface IExternalContext
{
    /// <summary>
    /// For interacting with the file system
    /// </summary>
    public IFileSystemHelper FileSystemHelper { get; }

    /// <summary>
    /// For interacting with external processes
    /// </summary>
    public IExternalProcessRunner ExternalProcessRunner { get; }

    /// <summary>
    /// For interacting with the console
    /// </summary>
    public IConsole Console { get; }
}

/// <summary>
/// The external context of a sequence.
/// </summary>
public sealed record ExternalContext(
    IFileSystemHelper FileSystemHelper,
    IExternalProcessRunner ExternalProcessRunner,
    IConsole Console) : IExternalContext
{
    /// <summary>
    /// The real external context
    /// </summary>
    public static readonly ExternalContext ConcreteInstance = new(
        ExternalProcesses.FileSystemHelper.Instance,
        ExternalProcesses.ExternalProcessRunner.Instance,
        new ConsoleAdapter()
    );
}

/// <summary>
/// The state monad that is passed between steps.
/// </summary>
public interface IStateMonad : IDisposable
{
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
    Result<Unit, IError> SetVariable<T>(VariableName key, T variable);

    /// <summary>
    /// Removes the variable if it exists.
    /// </summary>
    void RemoveVariable(VariableName key, bool dispose);
}

/// <summary>
/// The state monad that is passed between steps.
/// </summary>
public sealed class StateMonad : IStateMonad
{
    private readonly ConcurrentDictionary<VariableName, object> _stateDictionary = new();

    /// <summary>
    /// Create a new StateMonad
    /// </summary>
    public StateMonad(
        ILogger logger,
        SCLSettings settings,
        StepFactoryStore stepFactoryStore,
        IExternalContext externalContext)
    {
        Logger           = logger;
        Settings         = settings;
        StepFactoryStore = stepFactoryStore;
        ExternalContext  = externalContext;
    }

    /// <summary>
    /// The logger that steps will use to output messages.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The settings for this step.
    /// </summary>
    public SCLSettings Settings { get; }

    /// <summary>
    /// The external context
    /// </summary>
    public IExternalContext ExternalContext { get; }

    /// <summary>
    /// The step factory store. Maps from step names to step factories.
    /// </summary>
    public StepFactoryStore StepFactoryStore { get; }

    /// <summary>
    /// Gets all VariableNames and associated objects.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<VariableName, object>> GetState() => _stateDictionary;

    /// <summary>
    /// Gets the current value of this variable.
    /// </summary>
    public Result<T, IErrorBuilder> GetVariable<T>(VariableName key)
    {
        if (Disposed)
            throw new ObjectDisposedException("State Monad was disposed");

        var r = TryGetVariableFromDictionary<T>(key, _stateDictionary)
            .Bind(
                x =>
                    x.ToResult<T, IErrorBuilder>(new ErrorBuilder(ErrorCode.MissingVariable, key))
            );

        return r;
    }

    /// <summary>
    /// Tries to get a variable from a dictionary.
    /// </summary>
    public static Result<Maybe<T>, IErrorBuilder> TryGetVariableFromDictionary<T>(
        VariableName key,
        IReadOnlyDictionary<VariableName, object> dictionary)
    {
        if (!dictionary.TryGetValue(key, out var value))
            return Maybe<T>.None;

        if (value is T typedValue)
            return Maybe<T>.From(typedValue);

        return new ErrorBuilder(ErrorCode.WrongVariableType, key, typeof(T).Name);
    }

    /// <summary>
    /// Returns whether a particular variable has been set and not removed.
    /// </summary>
    public bool VariableExists(VariableName variable) => _stateDictionary.ContainsKey(variable);

    /// <summary>
    /// Creates or set the value of this variable.
    /// </summary>
    public Result<Unit, IError> SetVariable<T>(VariableName key, T variable)
    {
        if (Disposed)
            throw new ObjectDisposedException("State Monad was disposed");

        _stateDictionary
            .AddOrUpdate(key, _ => variable!, (_, _) => variable!);

        return Unit.Default;
    }

    /// <summary>
    /// Removes the variable if it exists.
    /// </summary>
    public void RemoveVariable(VariableName key, bool dispose)
    {
        if (Disposed)
            throw new ObjectDisposedException("State Monad was disposed");

        if (!_stateDictionary.Remove(key, out var variable))
            return;

        if (dispose && variable is IDisposable disposable)
            disposable.Dispose();
    }

    /// <summary>
    /// Has this State Monad been disposed
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Dispose of this State Monad
    /// </summary>
    public void Dispose()
    {
        if (!Disposed)
        {
            Disposed = true;

            foreach (var val in _stateDictionary.Values.OfType<IDisposable>())
            {
                val.Dispose();
            }
        }
    }
}

}
