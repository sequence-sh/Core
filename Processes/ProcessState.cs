using System.Collections.Concurrent;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// The state of a process.
    /// </summary>
    public sealed class ProcessState
    {
        private readonly ConcurrentDictionary<VariableName, object>  _stateDictionary = new ConcurrentDictionary<VariableName, object>();

        /// <summary>
        /// Create a new ProcessState
        /// </summary>
        public ProcessState(ILogger logger, IProcessSettings processSettings, IExternalProcessRunner externalProcessRunner)
        {
            Logger = logger;
            ProcessSettings = processSettings;
            ExternalProcessRunner = externalProcessRunner;
        }

        /// <summary>
        /// The logger that processes will use to output messages.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// The settings for this process.
        /// </summary>
        public IProcessSettings ProcessSettings { get; }

        /// <summary>
        /// The runner of external processes.
        /// </summary>
        public IExternalProcessRunner ExternalProcessRunner { get; }

        /// <summary>
        /// Get process settings of a particular type.
        /// </summary>
        public Result<T, IRunErrors> GetProcessSettings<T>(string processName) where T : IProcessSettings =>
            ProcessSettings.TryCast<T>()
                .MapFailure(x => new RunError(x, processName, null, ErrorCode.MissingProcessSettings) as IRunErrors);

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