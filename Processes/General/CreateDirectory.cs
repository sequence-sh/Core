using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Creates a new directory in the file system.
    /// </summary>
    public class CreateDirectoryProcessFactory : SimpleRunnableProcessFactory<CreateDirectory, Unit>
    {
        private CreateDirectoryProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<CreateDirectory, Unit> Instance { get; } = new CreateDirectoryProcessFactory();
    }

    /// <summary>
    /// Creates a new directory in the file system.
    /// </summary>
    public class CreateDirectory : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var pathResult = Path.Run(processState);
            if (pathResult.IsFailure)
                return pathResult.ConvertFailure<Unit>();

            var path = pathResult.Value;


            Maybe<IRunErrors> error;
            try
            {
                Directory.CreateDirectory(path);
                error = Maybe<IRunErrors>.None;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                error = Maybe<IRunErrors>.From(new RunError(e.Message, Name, null, ErrorCode.ExternalProcessError));
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (error.HasValue)
                return Result.Failure<Unit, IRunErrors>(error.Value);

            return Unit.Default;
        }

        /// <summary>
        /// The path to the directory to create.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> Path { get; set; } = null!;


        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => CreateDirectoryProcessFactory.Instance;
    }
}
