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
        public ScopedStateMonad(IStateMonad baseStateMonad, params KeyValuePair<VariableName, object>[] state)
        {
            BaseStateMonad = baseStateMonad;

            _scopedStateDictionary= new ConcurrentDictionary<VariableName, object>(state);
        }

        /// <inheritdoc />
        public void Dispose() {}

        private readonly ConcurrentDictionary<VariableName, object> _scopedStateDictionary;

        private IStateMonad BaseStateMonad { get; }

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
        public IEnumerable<KeyValuePair<VariableName, object>> GetState() => _scopedStateDictionary.Concat(BaseStateMonad.GetState());


        /// <inheritdoc />
        public Result<T, IErrorBuilder> GetVariable<T>(VariableName key)
        {
            if (_scopedStateDictionary.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;

                return new ErrorBuilder($"Variable '{key}' does not have type '{typeof(T)}'.", ErrorCode.WrongVariableType);
            }

            return BaseStateMonad.GetVariable<T>(key);
        }

        /// <inheritdoc />
        public bool VariableExists(VariableName variable) => _scopedStateDictionary.ContainsKey(variable) || BaseStateMonad.VariableExists(variable);

        /// <inheritdoc />
        public Result<Unit, IError> SetVariable<T>(VariableName key, T variable)
        {
            _scopedStateDictionary.AddOrUpdate(key, _ => variable!, (_1, _2) => variable!);

            return Unit.Default;
        }

        /// <inheritdoc />
        public void RemoveVariable(VariableName key, bool dispose)
        {
            if (_scopedStateDictionary.Remove(key, out var v))
            {
                if(v is IDisposable d)d.Dispose();
            }

            else
                BaseStateMonad.RemoveVariable(key, dispose);

        }
    }
}