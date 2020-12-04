using System.ComponentModel.DataAnnotations;
using System.IO;
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
    public sealed class FileWrite  : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var path = await Path.Run(stateMonad, cancellationToken);

            if (path.IsFailure) return path.ConvertFailure<Unit>();

            var stream = await Stream.Run(stateMonad, cancellationToken);

            if (stream.IsFailure) return stream.ConvertFailure<Unit>();

            stream.Value.Stream.Seek(0, SeekOrigin.Begin);

            var r = await stateMonad.FileSystemHelper.WriteFileAsync(path.Value, stream.Value.Stream, cancellationToken)
                .MapError(x=>x.WithLocation(this));

            return r;

        }

        /// <summary>
        /// The path of the file to write to.
        /// </summary>
        [StepProperty(Order = 0)]
        [Required]
        public IStep<string> Path { get; set; } = null!;

        /// <summary>
        /// The data to write.
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<DataStream> Stream { get; set; } = null!;


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
