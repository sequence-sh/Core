using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Reads text from a file.
    /// </summary>
    public sealed class ReadFile : CompoundStep<Stream>
    {
        /// <inheritdoc />
        public override async Task<Result<Stream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var path = await Path.Run(stateMonad, cancellationToken);

            if (path.IsFailure) return path.ConvertFailure<Stream>();


            var result = stateMonad.FileSystemHelper.ReadFile(path.Value)
                    .MapError(x=>x.WithLocation(this));

            return result;
        }


        /// <summary>
        /// The name of the file to read.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Path { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => ReadFileStepFactory.Instance;
    }

    /// <summary>
    /// Reads text from a file.
    /// </summary>
    public sealed class ReadFileStepFactory : SimpleStepFactory<ReadFile, string>
    {
        private ReadFileStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ReadFile, string> Instance { get; } = new ReadFileStepFactory();
    }
}