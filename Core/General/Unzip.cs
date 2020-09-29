using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Unzip a file in the file system.
    /// </summary>
    public class Unzip : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var data = ArchiveFilePath.Run(stateMonad)
                .Compose(() => DestinationDirectory.Run(stateMonad), () => OverwriteFiles.Run(stateMonad));

            if (data.IsFailure)
                return data.ConvertFailure<Unit>();

            Maybe<IRunErrors> error;
            try
            {
                ZipFile.ExtractToDirectory(data.Value.Item1, data.Value.Item2, data.Value.Item3);
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
        /// The path to the archive to unzip.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> ArchiveFilePath { get; set; } = null!;

        /// <summary>
        /// The directory to unzip to.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> DestinationDirectory { get; set; } = null!;

        /// <summary>
        /// Whether to overwrite files when unzipping.
        /// </summary>
        [StepProperty]
        [Required]
        [DefaultValueExplanation("false")]
        public IStep<bool> OverwriteFiles { get; set; } = new Constant<bool>(false);

        /// <inheritdoc />
        public override IStepFactory StepFactory => UnzipStepFactory.Instance;
    }

    /// <summary>
    /// Unzip a file in the file system.
    /// </summary>
    public class UnzipStepFactory : SimpleStepFactory<Unzip, Unit>
    {
        private UnzipStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<Unzip, Unit> Instance { get; } = new UnzipStepFactory();
    }
}
