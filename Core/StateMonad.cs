using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{

/// <summary>
/// The state monad that is passed between steps.
/// Mutable.
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
        IExternalContext externalContext,
        IReadOnlyDictionary<string, object> sequenceMetadata)
    {
        Logger           = logger;
        Settings         = settings;
        StepFactoryStore = stepFactoryStore;
        ExternalContext  = externalContext;
        SequenceMetadata = sequenceMetadata;
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
    /// The step factory store. Maps from step names to step factories.
    /// </summary>
    public StepFactoryStore StepFactoryStore { get; }

    /// <summary>
    /// The external context
    /// </summary>
    public IExternalContext ExternalContext { get; }

    /// <summary>
    /// Constant metadata for the entire sequence
    /// </summary>
    public IReadOnlyDictionary<string, object> SequenceMetadata { get; }

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

        if (typeof(T) == typeof(StringStream))
        {
            var ss = new StringStream(value.ToString()!);

            if (ss is T t)
            {
                return Maybe<T>.From(t);
            }
        }

        return new ErrorBuilder(ErrorCode.WrongVariableType, key, typeof(T).Name);
    }

    /// <summary>
    /// Returns whether a particular variable has been set and not removed.
    /// </summary>
    public bool VariableExists(VariableName variable) => _stateDictionary.ContainsKey(variable);

    /// <summary>
    /// Creates or set the value of this variable.
    /// </summary>
    public async Task<Result<Unit, IError>> SetVariableAsync<T>(
        VariableName key,
        T variable,
        bool disposeOld,
        IStep? callingStep)
    {
        if (Disposed)
            throw new ObjectDisposedException("State Monad was disposed");

        await RemoveVariableAsync(key, disposeOld, callingStep);

        _stateDictionary
            .AddOrUpdate(
                key,
                _ => variable!,
                (_, _) => variable!
            );

        return Unit.Default;
    }

    /// <summary>
    /// Removes the variable if it exists.
    /// </summary>
    public async Task RemoveVariableAsync(VariableName key, bool dispose, IStep? callingStep)
    {
        if (Disposed)
            throw new ObjectDisposedException("State Monad was disposed");

        if (!_stateDictionary.Remove(key, out var variable))
            return;

        if (dispose)
            await DisposeVariableAsync(variable, this);
    }

    internal static async Task DisposeVariableAsync(object variable, IStateMonad stateMonad)
    {
        if (variable is IStateDisposable stateDisposable)
            await stateDisposable.DisposeAsync(stateMonad);

        if (variable is IDisposable disposable)
            disposable.Dispose();
    }

    /// <summary>
    /// Has this State Monad been disposed
    /// </summary>
    public bool Disposed { get; private set; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!Disposed)
        {
            Disposed = true;

            foreach (var v in _stateDictionary.Values)
                await DisposeVariableAsync(v, this);
        }
    }
}

/// <summary>
/// An element which may require a reference to the state to dispose more effectively.
/// </summary>
public interface IStateDisposable
{
    /// <summary>
    /// Performs application defined functions associated with freeing resources
    /// </summary>
    /// <param name="state"></param>
    Task DisposeAsync(IStateMonad state);
}

}
