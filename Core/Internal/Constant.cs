using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step that returns a fixed value when run.
    /// </summary>
    public class Constant<T> : IStep<T>, IConstantStep
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
        public Result<T, IRunErrors> Run(StateMonad stateMonad) => Value!;

        /// <inheritdoc />
        public string Name => $"{Value}";

        /// <inheritdoc />
        public IFreezableStep Unfreeze() => new ConstantFreezableStep(Value!);

        /// <inheritdoc />
        public Result<T1, IRunErrors> Run<T1>(StateMonad stateMonad)
        {
            var r = Value!.TryConvert<T1>()
                .MapFailure(x => new RunError(x, Name, null, ErrorCode.InvalidCast) as IRunErrors);

            return r;
        }

        /// <inheritdoc />
        public Result<Unit, IRunErrors> Verify(ISettings settings) => Unit.Default;

        /// <inheritdoc />
        public Configuration? Configuration { get; set; } = null;

        /// <inheritdoc />
        public IEnumerable<IStepCombiner> StepCombiners => ArraySegment<IStepCombiner>.Empty;

        /// <inheritdoc />
        public Type OutputType => typeof(T);
    }
}