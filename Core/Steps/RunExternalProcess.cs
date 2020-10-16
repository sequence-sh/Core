using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Runs an external executable program.
    /// </summary>
    public sealed class RunExternalProcess : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IRunErrors>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var pathResult = await ProcessPath.Run(stateMonad, cancellationToken);
            if (pathResult.IsFailure) return pathResult.ConvertFailure<Unit>();

            List<string> arguments;

            if(Arguments == null)
                arguments = new List<string>();
            else
            {
                var argsResult = await Arguments.Run(stateMonad, cancellationToken);

                if (argsResult.IsFailure) return argsResult.ConvertFailure<Unit>();
                arguments = argsResult.Value;
            }

            var r =
                stateMonad.ExternalProcessRunner.RunExternalProcess(pathResult.Value,
                    stateMonad.Logger,
                    nameof(RunExternalProcess),
                    IgnoreNoneErrorHandler.Instance,
                    arguments).Result;

            return r;
        }



        /// <summary>
        /// The path to the external process
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<string> ProcessPath { get; set; } = null!;

        /// <summary>
        /// Arguments to the step.
        /// </summary>
        [StepProperty(Order = 2)]
        public IStep<List<string>>? Arguments { get; set; }


        /// <inheritdoc />
        public override IStepFactory StepFactory => RunExternalProcessStepFactory.Instance;
    }


    /// <summary>
    /// Runs an external executable program.
    /// </summary>
    public sealed class RunExternalProcessStepFactory : SimpleStepFactory<RunExternalProcess, Unit>
    {
        private RunExternalProcessStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<RunExternalProcess, Unit> Instance { get; } = new RunExternalProcessStepFactory();
    }
}