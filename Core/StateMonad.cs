using System.Collections.Concurrent;
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
        /// Get settings of a particular type.
        /// </summary>
        public Result<T, IError> GetSettings<T>(string stepName) where T : ISettings =>
            Settings.TryCast<T>()
                .MapFailure(x => new SingleError(x, stepName, null, ErrorCode.MissingStepSettings) as IError);

        /// <summary>
        /// Gets the current value of this variable.
        /// </summary>
        public Result<T,IError> GetVariable<T>(VariableName key, string stepName)
        {
            if (_stateDictionary.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;

                return new SingleError($"Variable '{key}' does not have type '{typeof(T)}'.", stepName, null, ErrorCode.WrongVariableType);
            }

            return new SingleError($"Variable '{key}' does not exist.", stepName, null, ErrorCode.MissingVariable);


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