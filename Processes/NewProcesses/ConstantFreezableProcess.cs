using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// A process that returns a fixed value when run.
    /// </summary>
    public sealed class ConstantFreezableProcess : IFreezableProcess
    {
        /// <summary>
        /// Creates a new ConstantFreezableProcess.
        /// </summary>
        /// <param name="value"></param>
        public ConstantFreezableProcess(object value) => Value = value;

        /// <summary>
        /// The value that this will return when run.
        /// </summary>
        public object Value { get; }


        /// <inheritdoc />
        public Result<IRunnableProcess> TryFreeze(ProcessContext _)
        {
            Type elementType = Value.GetType();
            Type processType = typeof(ConstantRunnableProcess<>).MakeGenericType(elementType);
            var process = Activator.CreateInstance(processType, Value);

            //TODO check for exceptions here?

            var runnableProcess = (IRunnableProcess) process!;

            return Result.Success(runnableProcess);
        }

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(string name, ITypeReference type)>> TryGetVariablesSet => ImmutableList<(string name, ITypeReference type)>.Empty;

        /// <inheritdoc />
        public string ProcessName => NameHelper.GetConstantName(Value);

        /// <inheritdoc />
        public Result<ITypeReference> TryGetOutputTypeReference() => new ActualTypeReference(Value.GetType());
    }
}