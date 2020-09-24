using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.Internal
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
        public T Value { get; }

        /// <inheritdoc />
        public Result<T, IRunErrors> Run(ProcessState processState) => Value!;

        /// <inheritdoc />
        public string Name => $"{Value}";

        /// <inheritdoc />
        public IFreezableProcess Unfreeze() => new ConstantFreezableProcess(Value!);

        /// <inheritdoc />
        public Result<T1, IRunErrors> Run<T1>(ProcessState processState)
        {
            var r = Value!.TryConvert<T1>()
                .MapFailure(x => new RunError(x, Name, null, ErrorCode.InvalidCast) as IRunErrors);

            return r;
        }

        /// <inheritdoc />
        public Result<Unit, IRunErrors> Verify(IProcessSettings settings) => Unit.Default;

        /// <inheritdoc />
        public ProcessConfiguration? ProcessConfiguration { get; set; } = null;

        /// <inheritdoc />
        public IEnumerable<IProcessCombiner> ProcessCombiners => ArraySegment<IProcessCombiner>.Empty;

        /// <inheritdoc />
        public Type OutputType => typeof(T);
    }
}