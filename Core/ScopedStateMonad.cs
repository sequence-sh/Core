using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
/// A state monad with additional state defined
/// </summary>
public sealed class ScopedStateMonad : IStateMonad
{
    /// <summary>
    /// Create a new ScopedStateMonad
    /// </summary>
    public ScopedStateMonad(
        IStateMonad baseStateMonad,
        ImmutableDictionary<VariableName, object> fixedState,
        params KeyValuePair<VariableName, object>[] state)
    {
        _fixedState            = fixedState;
        BaseStateMonad         = baseStateMonad;
        _scopedStateDictionary = new ConcurrentDictionary<VariableName, object>(state);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var value in _scopedStateDictionary.Values)
            StateMonad.DisposeVariable(value, this);
    }

    private readonly ConcurrentDictionary<VariableName, object> _scopedStateDictionary;

    private IStateMonad BaseStateMonad { get; }

    private readonly ImmutableDictionary<VariableName, object> _fixedState;

    /// <inheritdoc />
    public object SequenceMetadata => BaseStateMonad.SequenceMetadata;

    /// <inheritdoc />
    public ILogger Logger => BaseStateMonad.Logger;

    /// <inheritdoc />
    public SCLSettings Settings => BaseStateMonad.Settings;

    /// <inheritdoc />
    public IExternalContext ExternalContext => BaseStateMonad.ExternalContext;

    /// <inheritdoc />
    public StepFactoryStore StepFactoryStore => BaseStateMonad.StepFactoryStore;

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<VariableName, object>> GetState() =>
        _scopedStateDictionary.Concat(_fixedState);

    /// <inheritdoc />
    public Result<T, IErrorBuilder> GetVariable<T>(VariableName key)
    {
        var r1 = StateMonad.TryGetVariableFromDictionary<T>(key, _scopedStateDictionary);

        if (r1.IsFailure)
            return r1.ConvertFailure<T>();

        if (r1.Value.HasValue)
            return r1.Value.Value;

        var r2 = StateMonad
            .TryGetVariableFromDictionary<T>(key, _fixedState)
            .Bind(
                x => x.ToResult<T, IErrorBuilder>(new ErrorBuilder(ErrorCode.MissingVariable, key))
            );

        return r2;
    }

    /// <inheritdoc />
    public bool VariableExists(VariableName variable) =>
        _scopedStateDictionary.ContainsKey(variable) ||
        _fixedState.ContainsKey(variable);

    /// <inheritdoc />
    public Result<Unit, IError> SetVariable<T>(
        VariableName key,
        T variable,
        bool disposeOld,
        IStep? callingStep)
    {
        _scopedStateDictionary.AddOrUpdate(
            key,
            _ => variable!,
            (_, old) =>
            {
                if (disposeOld)
                    StateMonad.DisposeVariable(old, this);

                return variable!;
            }
        );

        if (_fixedState.ContainsKey(key))
            Logger.LogSituation(
                LogSituation.SetVariableOutOfScope,
                callingStep,
                this,
                new[] { key }
            );

        return Unit.Default;
    }

    /// <inheritdoc />
    public void RemoveVariable(VariableName key, bool dispose, IStep? callingStep)
    {
        if (_scopedStateDictionary.Remove(key, out var v) && dispose)
        {
            StateMonad.DisposeVariable(v, this);
        }

        if (_fixedState.ContainsKey(key))
            Logger.LogSituation(
                LogSituation.RemoveVariableOutOfScope,
                callingStep,
                this,
                new[] { key }
            );
    }
}

}
