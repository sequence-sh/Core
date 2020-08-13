using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// A process that returns a fixed value when run.
    /// </summary>
    public class Constant<T> : IRunnableProcess<T>
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
    }
}