using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Unzip a file in the file system.
    /// </summary>
    public class UnzipProcessFactory : SimpleRunnableProcessFactory<Unzip, Unit>
    {
        private UnzipProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<Unzip, Unit> Instance { get; } = new UnzipProcessFactory();
    }

    /// <summary>
    /// Unzip a file in the file system.
    /// </summary>
    public class Unzip : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var data = ArchiveFilePath.Run(processState)
                .Compose(() => DestinationDirectory.Run(processState), () => OverwriteFiles.Run(processState));

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
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> ArchiveFilePath { get; set; } = null!;

        /// <summary>
        /// The directory to unzip to.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> DestinationDirectory { get; set; } = null!;

        /// <summary>
        /// Whether to overwrite files when unzipping.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        [DefaultValueExplanation("false")]
        public IRunnableProcess<bool> OverwriteFiles { get; set; } = new Constant<bool>(false);

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => UnzipProcessFactory.Instance;
    }
}
