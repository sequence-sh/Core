using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step that can be run.
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// The name of this step.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Convert this Step into a FreezableStep.
        /// </summary>
        IFreezableStep Unfreeze();

        /// <summary>
        /// Run this step and return the result, assuming it is the specified type.
        /// </summary>
         Task<Result<T, IError>> Run<T>(IStateMonad stateMonad, CancellationToken cancellationToken);

        /// <summary>
        /// Verify that this step can be run with the current settings.
        /// </summary>
        public Result<Unit, IError> Verify(ISettings settings);

        /// <summary>
        /// Configuration for this step.
        /// </summary>
        Configuration? Configuration { get; set; }

        /// <summary>
        /// The output type. Will be the generic type in IStep&lt;T&gt;
        /// </summary>
        Type OutputType { get; }

        /// <summary>
        /// SerializeAsync this step.
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task<string> SerializeAsync(CancellationToken cancellationToken);

    }

    /// <summary>
    /// A step that can be run.
    /// </summary>
    public interface IStep<T> : IStep
    {
        /// <summary>
        /// Run this step and return the result.
        /// </summary>
        Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken);
    }
}