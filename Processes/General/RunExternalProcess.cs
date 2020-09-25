using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Runs an external executable program.
    /// </summary>
    public sealed class RunExternalProcessFactory : SimpleRunnableProcessFactory<RunExternalProcess, Unit>
    {
        private RunExternalProcessFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<RunExternalProcess, Unit> Instance { get; } = new RunExternalProcessFactory();
    }


    /// <summary>
    /// Runs an external executable program.
    /// </summary>
    public sealed class RunExternalProcess : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var pathResult = ProcessPath.Run(processState);
            if (pathResult.IsFailure) return pathResult.ConvertFailure<Unit>();

            List<string> arguments;

            if(Arguments == null)
                arguments = new List<string>();
            else
            {
                var argsResult = Arguments.Run(processState);

                if (argsResult.IsFailure) return argsResult.ConvertFailure<Unit>();
                arguments = argsResult.Value;
            }

            var r =
                processState.ExternalProcessRunner.RunExternalProcess(pathResult.Value,
                    processState.Logger,
                    nameof(RunExternalProcess),
                    IgnoreNoneErrorHandler.Instance,
                    arguments).Result;

            return r;
        }



        /// <summary>
        /// The path to the external process
        /// </summary>
        [RunnableProcessProperty(Order = 1)]
        [Required]
        public IRunnableProcess<string> ProcessPath { get; set; } = null!;

        /// <summary>
        /// Arguments to the process.
        /// </summary>
        [RunnableProcessProperty(Order = 2)]
        public IRunnableProcess<List<string>>? Arguments { get; set; }


        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => RunExternalProcessFactory.Instance;
    }
}