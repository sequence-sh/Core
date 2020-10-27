using System.Collections.Concurrent;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{
    /// <summary>
    /// The state monad that is passed between steps.
    /// </summary>
    public sealed class StateMonad
    {
        private readonly ConcurrentDictionary<VariableName, object>  _stateDictionary = new ConcurrentDictionary<VariableName, object>();

        /// <summary>
        /// Create a new StateMonad
        /// </summary>
        public StateMonad(ILogger logger, ISettings settings, IExternalProcessRunner externalProcessRunner, StepFactoryStore stepFactoryStore)
        {
            Logger = logger;
            Settings = settings;
            ExternalProcessRunner = externalProcessRunner;
            StepFactoryStore = stepFactoryStore;
        }

        /// <summary>
        /// The logger that steps will use to output messages.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// The settings for this step.
        /// </summary>
        public ISettings Settings { get; }

        /// <summary>
        /// The runner of external processes.
        /// </summary>
        public IExternalProcessRunner ExternalProcessRunner { get; }

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
        /// Get settings of a particular type.
        /// </summary>
        public Result<T, IErrorBuilder> GetSettings<T>() where T : ISettings =>
            Settings.TryCast<T>()
                .MapError(x => new ErrorBuilder(x, ErrorCode.MissingStepSettings) as IErrorBuilder);

        /// <summary>
        /// Gets the current value of this variable.
        /// </summary>
        public Result<T,IErrorBuilder> GetVariable<T>(VariableName key)
        {
            if (_stateDictionary.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;

                return new ErrorBuilder($"Variable '{key}' does not have type '{typeof(T)}'.", ErrorCode.WrongVariableType);
            }

            return new ErrorBuilder($"Variable '{key}' does not exist.", ErrorCode.MissingVariable);


        }

        /// <summary>
        /// Creates or set the value of this variable.
        /// </summary>
        public Result<Unit, IError> SetVariable<T>(VariableName key, T variable)
        {
            _stateDictionary
                .AddOrUpdate(key, _ => variable!, (_1, _2) => variable!);

            return Unit.Default;
        }
    }
}