using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Reads text from a file.
    /// </summary>
    public sealed class ReadFile : CompoundStep<DataStream>
    {
        /// <inheritdoc />
        public override async Task<Result<DataStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var path = await Path.Run(stateMonad, cancellationToken);

            if (path.IsFailure) return path.ConvertFailure<DataStream>();


            var result = stateMonad.FileSystemHelper.ReadFile(path.Value)
                    .MapError(x=>x.WithLocation(this))
                    .Map(x=> new DataStream(x, EncodingEnum.UTF8)); //TODO fix

            return result;
        }


        /// <summary>
        /// The name of the file to read.
        /// </summary>
        [StepProperty(1)]
        [Required]
        [Alias("FromPath")]
        public IStep<string> Path { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => ReadFileStepFactory.Instance;
    }

    /// <summary>
    /// Reads text from a file.
    /// </summary>
    public sealed class ReadFileStepFactory : SimpleStepFactory<ReadFile, DataStream>
    {
        private ReadFileStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ReadFile, DataStream> Instance { get; } = new ReadFileStepFactory();
    }
}