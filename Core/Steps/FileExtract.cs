using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Extract a file in the file system.
    /// </summary>
    public class FileExtract : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var data = await ArchiveFilePath.Run(stateMonad, cancellationToken)
                .Compose(() => Destination.Run(stateMonad, cancellationToken), () => Overwrite.Run(stateMonad, cancellationToken));

            if (data.IsFailure)
                return data.ConvertFailure<Unit>();

            var result =
                stateMonad.FileSystemHelper.ExtractToDirectory(data.Value.Item1, data.Value.Item2, data.Value.Item3);

            return result.MapError(x=>x.WithLocation(this));

        }


        /// <summary>
        /// The path to the archive to extract.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> ArchiveFilePath { get; set; } = null!;

        /// <summary>
        /// The directory to extract to.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Destination { get; set; } = null!;

        /// <summary>
        /// Whether to overwrite files when extracting.
        /// </summary>
        [StepProperty]
        [DefaultValueExplanation("false")]
        public IStep<bool> Overwrite { get; set; } = new Constant<bool>(false);

        /// <inheritdoc />
        public override IStepFactory StepFactory => FileExtractStepFactory.Instance;
    }

    /// <summary>
    /// Extract a file in the file system.
    /// </summary>
    public class FileExtractStepFactory : SimpleStepFactory<FileExtract, Unit>
    {
        private FileExtractStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<FileExtract, Unit> Instance { get; } = new FileExtractStepFactory();
    }
}
