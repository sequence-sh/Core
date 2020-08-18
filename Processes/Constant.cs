using System;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// A process that returns a fixed value when run.
    /// </summary>
    public interface IConstantRunnableProcess : IRunnableProcess
    {
        /// <summary>
        /// The output type.
        /// </summary>
        Type OutputType { get; }
    }


    /// <summary>
    /// A process that returns a fixed value when run.
    /// </summary>
    public class Constant<T> : IRunnableProcess<T>, IConstantRunnableProcess
    {
        /// <summary>
        /// Creates a new Constant.
        /// </summary>
        /// <param name="value"></param>
        public Constant(T value) => Value = value;

        /// <summary>
        /// The value that this will return when run.
        /// </summary>
        public T Value { get; set; }

        /// <inheritdoc />
        public Result<T> Run(ProcessState processState) => Value!;

        /// <inheritdoc />
        public string Name => $"{Value}";

        /// <inheritdoc />
        public IFreezableProcess Unfreeze() => new ConstantFreezableProcess(Value!);

        /// <inheritdoc />
        public Result<object> RunUntyped(ProcessState processState) => Run(processState);

        /// <inheritdoc />
        public Type OutputType => typeof(T);
    }
}