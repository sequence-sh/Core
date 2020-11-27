using System.ComponentModel.DataAnnotations;
using System.IO;
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
    /// Writes a file to the local file system.
    /// </summary>
    public sealed class WriteFile  : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var data = await Folder.Run(stateMonad, cancellationToken).Compose(() => FileName.Run(stateMonad, cancellationToken),()=> Text.Run(stateMonad, cancellationToken));

            if (data.IsFailure)
                return data.ConvertFailure<Unit>();


            var path = Path.Combine(data.Value.Item1, data.Value.Item2);
            var stream = data.Value.Item3;

            stream.Seek(0, SeekOrigin.Begin);

            var r = await stateMonad.FileSystemHelper.WriteFileAsync(path, stream, cancellationToken)
                .MapError(x=>x.WithLocation(this));

            return r;

        }

        /// <summary>
        /// The name of the file to write to.
        /// </summary>
        [StepProperty(Order = 0)]
        [Required]
        public IStep<string> FileName { get; set; } = null!;

        /// <summary>
        /// The name of the folder.
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<string> Folder { get; set; } = null!;

        /// <summary>
        /// The text to write.
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<Stream> Text { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => WriteFileStepFactory.Instance;
    }

    /// <summary>
    /// Writes a file to the local file system.
    /// </summary>
    public sealed class WriteFileStepFactory : SimpleStepFactory<WriteFile, Unit>
    {
        private WriteFileStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<WriteFile, Unit> Instance { get; } = new WriteFileStepFactory();
    }
}
