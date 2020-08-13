using System.Collections.Concurrent;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// The state of a process.
    /// </summary>
    public sealed class ProcessState
    {
        //TODO IObservable?

        private readonly ConcurrentDictionary<VariableName, object>  _stateDictionary = new ConcurrentDictionary<VariableName, object>();

        /// <summary>
        /// Create a new ProcessState
        /// </summary>
        public ProcessState(ILogger logger) => Logger = logger;

        /// <summary>
        /// The logger that processes will use to output messages.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets the current value of this variable.
        /// </summary>
        public Result<T> GetVariable<T>(VariableName key)
        {
            if (_stateDictionary.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;

                return Error<T>($"Variable '{key}' does not have type '{typeof(T)}'.");
            }

            return Error<T>($"Variable '{key}' does not exist.");


        }

        /// <summary>
        /// Creates or set the value of this variable.
        /// </summary>
        public Result SetVariable<T>(VariableName key, T variable)
        {
            _stateDictionary.AddOrUpdate(key, _ => variable!, (_1, _2) => variable!);

            return Result.Success();
        }

        private static Result<T> Error<T>(string message) => Result.Failure<T>(message);
    }
}