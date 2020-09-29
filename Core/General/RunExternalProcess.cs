using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Runs an external executable program.
    /// </summary>
    public sealed class RunExternalProcess : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var pathResult = ProcessPath.Run(stateMonad);
            if (pathResult.IsFailure) return pathResult.ConvertFailure<Unit>();

            List<string> arguments;

            if(Arguments == null)
                arguments = new List<string>();
            else
            {
                var argsResult = Arguments.Run(stateMonad);

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
}