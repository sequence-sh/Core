using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.ExternalProcesses;
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
        foreach (var disposable in _scopedStateDictionary.Values.OfType<IDisposable>())
        {
            disposable.Dispose();
        }
    }

    private readonly ConcurrentDictionary<VariableName, object> _scopedStateDictionary;

    private IStateMonad BaseStateMonad { get; }

    private readonly ImmutableDictionary<VariableName, object> _fixedState;

    /// <inheritdoc />
    public ILogger Logger => BaseStateMonad.Logger;

    /// <inheritdoc />
    public ISettings Settings => BaseStateMonad.Settings;

    /// <inheritdoc />
    public IExternalProcessRunner ExternalProcessRunner => BaseStateMonad.ExternalProcessRunner;

    /// <inheritdoc />
    public IFileSystemHelper FileSystemHelper => BaseStateMonad.FileSystemHelper;

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
                x => x.ToResult<T, IErrorBuilder>(
                    new ErrorBuilder_Core(ErrorCode_Core.MissingVariable, key)
                )
            );

        return r2;
    }

    /// <inheritdoc />
    public bool VariableExists(VariableName variable) =>
        _scopedStateDictionary.ContainsKey(variable) ||
        _fixedState.ContainsKey(variable);

    /// <inheritdoc />
    public Result<Unit, IError> SetVariable<T>(VariableName key, T variable)
    {
        _scopedStateDictionary.AddOrUpdate(key, _ => variable!, (_1, _2) => variable!);

        if (_fixedState.ContainsKey(key))
            Logger.LogSituation(LogSituation_Core.SetVariableOutOfScope, new object[] { key });

        return Unit.Default;
    }

    /// <inheritdoc />
    public void RemoveVariable(VariableName key, bool dispose)
    {
        if (_scopedStateDictionary.Remove(key, out var v))
            if (v is IDisposable d)
                d.Dispose();

        if (_fixedState.ContainsKey(key))
            Logger.LogSituation(LogSituation_Core.RemoveVariableOutOfScope, new object[] { key });
    }
}

}
