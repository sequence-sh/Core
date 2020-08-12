using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// A process that returns a fixed value when run.
    /// </summary>
    public class ConstantRunnableProcess<T> : IRunnableProcess<T>
    {
        /// <summary>
        /// Creates a new ConstantRunnableProcess.
        /// </summary>
        /// <param name="value"></param>
        public ConstantRunnableProcess(T value) => Value = value;

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