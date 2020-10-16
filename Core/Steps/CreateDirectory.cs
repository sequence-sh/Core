using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        public override async Task<Result<Unit, IRunErrors>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var path = await Path.Run(stateMonad, cancellationToken);

            if (path.IsFailure)
                return path.ConvertFailure<Unit>();


            Result<Unit, IRunErrors> r;

            try
            {
                Directory.CreateDirectory(path.Value);
                r = Result.Success<Unit, IRunErrors>(Unit.Default);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                r = new RunError(e.Message, nameof(CreateDirectory), null, ErrorCode.ExternalProcessError);
            }

            return r;
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
