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
    [Alias("ReadFromFile")]
    public sealed class FileRead : CompoundStep<StringStream>
    {
        /// <inheritdoc />
        public override async Task<Result<StringStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var path = await Path.Run(stateMonad, cancellationToken)
                .Map(async x => await x.GetStringAsync());

            if (path.IsFailure) return path.ConvertFailure<StringStream>();

            var encoding = await Encoding.Run(stateMonad, cancellationToken);

            if (encoding.IsFailure) return encoding.ConvertFailure<StringStream>();


            var result = stateMonad.FileSystemHelper.ReadFile(path.Value)
                    .MapError(x => x.WithLocation(this))
                    .Map(x => new StringStream(x, encoding.Value)); //TODO fix

            return result;
        }

        /// <summary>
        /// The name of the file to read.
        /// </summary>
        [StepProperty(1)]
        [Required]
        [Alias("File")]
        public IStep<StringStream> Path { get; set; } = null!;

        /// <summary>
        /// 
        /// How the file is encoded.
        /// </summary>
        [StepProperty(2)]
        [DefaultValueExplanation("UTF8 no BOM")]
        public IStep<EncodingEnum> Encoding { get; set; } = new EnumConstant<EncodingEnum>(EncodingEnum.UTF8);

        /// <inheritdoc />
        public override IStepFactory StepFactory => FileReadStepFactory.Instance;
    }

    /// <summary>
    /// Reads text from a file.
    /// </summary>
    public sealed class FileReadStepFactory : SimpleStepFactory<FileRead, StringStream>
    {
        private FileReadStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<FileRead, StringStream> Instance { get; } = new FileReadStepFactory();
    }
}