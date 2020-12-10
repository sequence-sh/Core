using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
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
            var archivePathResult = await ArchiveFilePath.Run(stateMonad, cancellationToken)
                .Map(async x=> await x.GetStringAsync());

            if (archivePathResult.IsFailure) return archivePathResult.ConvertFailure<Unit>();

            var destinationResult = await Destination.Run(stateMonad, cancellationToken)
                .Map(async x=> await x.GetStringAsync());

            if (destinationResult.IsFailure) return destinationResult.ConvertFailure<Unit>();

            var overwriteResult = await Overwrite.Run(stateMonad, cancellationToken);

            if (overwriteResult.IsFailure) return overwriteResult.ConvertFailure<Unit>();

            var result =
                stateMonad.FileSystemHelper.ExtractToDirectory(archivePathResult.Value, destinationResult.Value, overwriteResult.Value);

            return result.MapError(x=>x.WithLocation(this));

        }


        /// <summary>
        /// The path to the archive to extract.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<StringStream> ArchiveFilePath { get; set; } = null!;

        /// <summary>
        /// The directory to extract to.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<StringStream> Destination { get; set; } = null!;

        /// <summary>
        /// Whether to overwrite files when extracting.
        /// </summary>
        [StepProperty(3)]
        [DefaultValueExplanation("false")]
        public IStep<bool> Overwrite { get; set; } = new BoolConstant(false);

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
