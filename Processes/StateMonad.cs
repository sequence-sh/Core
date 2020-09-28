using System.Collections.Concurrent;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes
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
        public StateMonad(ILogger logger, ISettings settings, IExternalProcessRunner externalProcessRunner)
        {
            Logger = logger;
            Settings = settings;
            ExternalProcessRunner = externalProcessRunner;
        }

        /// <summary>
        /// The logger that processes will use to output messages.
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
        /// Get settings of a particular type.
        /// </summary>
        public Result<T, IRunErrors> GetSettings<T>(string stepName) where T : ISettings =>
            Settings.TryCast<T>()
                .MapFailure(x => new RunError(x, stepName, null, ErrorCode.MissingProcessSettings) as IRunErrors);

        /// <summary>
        /// Gets the current value of this variable.
        /// </summary>
        public Result<T,IRunErrors> GetVariable<T>(VariableName key, string processName)
        {
            if (_stateDictionary.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;

                return new RunError($"Variable '{key}' does not have type '{typeof(T)}'.", processName, null, ErrorCode.WrongVariableType);
            }

            return new RunError($"Variable '{key}' does not exist.", processName, null, ErrorCode.MissingVariable);


        }

        /// <summary>
        /// Creates or set the value of this variable.
        /// </summary>
        public Result<Unit, IRunErrors> SetVariable<T>(VariableName key, T variable)
        {
            _stateDictionary
                .AddOrUpdate(key, _ => variable!, (_1, _2) => variable!);

            return Unit.Default;
        }
    }
}