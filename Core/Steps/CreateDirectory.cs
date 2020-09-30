using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Creates a new directory in the file system.
    /// </summary>
    public class CreateDirectory : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var pathResult = Path.Run(stateMonad);
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
        [StepProperty]
        [Required]
        public IStep<string> Path { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => CreateDirectoryStepFactory.Instance;
    }

    /// <summary>
    /// Creates a new directory in the file system.
    /// </summary>
    public class CreateDirectoryStepFactory : SimpleStepFactory<CreateDirectory, Unit>
    {
        private CreateDirectoryStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<CreateDirectory, Unit> Instance { get; } = new CreateDirectoryStepFactory();
    }
}
