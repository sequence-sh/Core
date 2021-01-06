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
    /// Writes a file to the local file system.
    /// </summary>
    [Alias("WriteToFile")]
    public sealed class FileWrite : CompoundStep<Unit>
    {
        /// <inheritdoc />
        protected override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var path = await Path.Run(stateMonad, cancellationToken)
                .Map(async x => await x.GetStringAsync());

            if (path.IsFailure) return path.ConvertFailure<Unit>();

            var stringStreamResult = await Stream.Run(stateMonad, cancellationToken);

            if (stringStreamResult.IsFailure) return stringStreamResult.ConvertFailure<Unit>();

            var stream = stringStreamResult.Value.GetStream().stream;

            var r = await stateMonad.FileSystemHelper.WriteFileAsync(path.Value, stream, cancellationToken)
                .MapError(x => x.WithLocation(this));

            await stream.DisposeAsync();

            return r;

        }

        /// <summary>
        /// The data to write to file.
        /// </summary>
        [StepProperty(1)]
        [Required]
        [Alias("Data")]
        public IStep<StringStream> Stream { get; set; } = null!;

        /// <summary>
        /// The path of the file to write to.
        /// </summary>
        [StepProperty(2)]
        [Required]
        [Log(LogOutputLevel.Trace)]
        public IStep<StringStream> Path { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => FileWriteStepFactory.Instance;
    }

    /// <summary>
    /// Writes a file to the local file system.
    /// </summary>
    public sealed class FileWriteStepFactory : SimpleStepFactory<FileWrite, Unit>
    {
        private FileWriteStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<FileWrite, Unit> Instance { get; } = new FileWriteStepFactory();
    }
}
