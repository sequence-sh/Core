using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
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
        public string Name => $"{Value}";

        /// <inheritdoc />
        public IFreezableStep Unfreeze() => new ConstantFreezableStep(Value!);

        /// <inheritdoc />
#pragma warning disable 1998
        public async Task<Result<T, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken) => Value!;

        /// <inheritdoc />
        public async Task<Result<T1, IError>>  Run<T1>(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var r = Value!.TryConvert<T1>()
                .MapError(x => new SingleError(x, ErrorCode.InvalidCast, new StepErrorLocation(this), null) as IError);

            return r;
        }
        #pragma warning restore 1998

        /// <inheritdoc />
        public Result<Unit, IError> Verify(ISettings settings) => Unit.Default;

        /// <inheritdoc />
        public Configuration? Configuration { get; set; } = null;

        /// <inheritdoc />
        public IEnumerable<IStepCombiner> StepCombiners => ArraySegment<IStepCombiner>.Empty;

        /// <inheritdoc />
        public Type OutputType => typeof(T);
    }
}