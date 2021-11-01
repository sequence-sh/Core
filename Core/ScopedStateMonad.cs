using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        Maybe<VariableName> automaticVariable,
        params KeyValuePair<VariableName, object>[] state)
    {
        _fixedState            = fixedState;
        AutomaticVariable      = automaticVariable;
        BaseStateMonad         = baseStateMonad;
        _scopedStateDictionary = new ConcurrentDictionary<VariableName, object>(state);
    }

    private readonly ConcurrentDictionary<VariableName, object> _scopedStateDictionary;

    private IStateMonad BaseStateMonad { get; }

    private readonly ImmutableDictionary<VariableName, object> _fixedState;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> SequenceMetadata => BaseStateMonad.SequenceMetadata;

    /// <inheritdoc />
    public ILogger Logger => BaseStateMonad.Logger;

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
            return r1.Value.GetValueOrThrow();

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
    public async Task<Result<Unit, IError>> SetVariableAsync<T>(
        VariableName key,
        T variable,
        bool disposeOld,
        IStep? callingStep)
    {
        await RemoveVariableAsync(key, disposeOld, callingStep);

        _scopedStateDictionary.AddOrUpdate(
            key,
            _ => variable!,
            (_, _) => variable!
        );

        return Unit.Default;
    }

    /// <inheritdoc />
    public async Task RemoveVariableAsync(VariableName key, bool dispose, IStep? callingStep)
    {
        if (_scopedStateDictionary.Remove(key, out var v) && dispose)
        {
            await StateMonad.DisposeVariableAsync(v, this);
        }

        if (_fixedState.ContainsKey(key))
            Logger.LogSituation(
                LogSituation.RemoveVariableOutOfScope,
                callingStep,
                this,
                new[] { key }
            );
    }

    /// <inheritdoc />
    public Maybe<VariableName> AutomaticVariable { get; }

    /// <inheritdoc />
    public Entity Settings => BaseStateMonad.Settings;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (var value in _scopedStateDictionary.Values)
            await StateMonad.DisposeVariableAsync(value, this);
    }
}

}
