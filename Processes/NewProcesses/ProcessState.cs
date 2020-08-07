using System.Collections.Concurrent;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// The state of a process.
    /// </summary>
    public sealed class ProcessState
    {
        //TODO logger
        //TODO IObservable

        private readonly ConcurrentDictionary<string, object>  _stateDictionary = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Gets the current value of this variable.
        /// </summary>
        public Result<T> GetVariable<T>(string key)
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
        public Result SetVariable<T>(string key, T variable)
        {
            _stateDictionary.AddOrUpdate(key, _ => variable!, (_1, _2) => variable!);

            return Result.Success();
        }

        private Result<T> Error<T>(string message)
        {
            //TODO log error

            return Result.Failure<T>(message);
        }

    }
}