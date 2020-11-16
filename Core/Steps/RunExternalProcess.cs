using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Runs an external executable program.
    /// </summary>
    public sealed class RunExternalProcess : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
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

            var encodingResult = await Encoding.Run(stateMonad, cancellationToken);
            if (encodingResult.IsFailure) return encodingResult.ConvertFailure<Unit>();


            var r = await
                stateMonad.ExternalProcessRunner.RunExternalProcess(pathResult.Value,
                    stateMonad.Logger,
                    IgnoreNoneErrorHandler.Instance,
                    arguments, encodingResult.Value.Convert(), cancellationToken).MapError(x=>x.WithLocation(this));

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
        [DefaultValueExplanation("No arguments")]
        public IStep<List<string>>? Arguments { get; set; }

        /// <summary>
        /// Encoding to use for the process output.
        /// </summary>
        [StepProperty(Order = 3)]
        [DefaultValueExplanation("Default encoding")]
        public IStep<EncodingEnum> Encoding { get; set; } = new Constant<EncodingEnum>(EncodingEnum.Default);


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